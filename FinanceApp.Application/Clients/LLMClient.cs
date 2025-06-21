using System.Text.Json;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Enums;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace FinanceApp.Application.Clients;

public class LLMClient : ILLMClient
{
  private readonly ILogger<LLMClient> _logger;
  private readonly ChatClient _client;

  public LLMClient(
    ChatClient client,
    ILogger<LLMClient> logger)
  {
    _client = client;
    _logger = logger;
  }

  public async Task<Result<ExchangeRateResponseDto>> GetExchangeDataAsync(CurrencyEnum targetCurrency)
  {
    var prompt = "You are a financial API that always answers exchange rate in the same JSON format as a list or array. " +
                 "Provide the current foreign exchange rates as a JSON object with a base currency and rates for USD, EUR, GBP, and HUF. The JSON should look like this: " +
                  $"{{ \"base\": \"{targetCurrency}\", \"rates\": {{ \"USD\": 0.0028, \"EUR\": 0.0024, \"GBP\": 0.0021, \"HUF\": 1 }} }}" +
                  "Only return the JSON response without any additional text or explanation. Without line breaks or markdown code blocks. ";

    ChatCompletion completion = await _client.CompleteChatAsync(prompt);

    var responseText = completion.Content[0].Text;

    JsonSerializerOptions options = new()
    {
      PropertyNameCaseInsensitive = true,
      WriteIndented = false
    };

    var exchangeRateResponse = JsonSerializer.Deserialize<ExchangeRateResponseDto>(responseText, options);

    if (exchangeRateResponse == null)
    {
      _logger.LogError("Failed to deserialize exchange rate response: {ResponseText}", responseText);
      return Result.Failure<ExchangeRateResponseDto>(ApplicationError.InvalidExchangeRateResponseError());
    }

    return Result.Success(exchangeRateResponse);
  }
}
