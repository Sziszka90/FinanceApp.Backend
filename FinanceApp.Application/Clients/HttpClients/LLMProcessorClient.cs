using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Application.Dtos.LLMProcessorDtos;
using FinanceApp.Application.Clients.HttpClients;
using FinanceApp.Application.LLMProcessorDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.Models.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp.Application.Clients;

public class LLMProcessorClient : HttpClientBase<ILLMProcessorClient>, ILLMProcessorClient
{
  private readonly ILogger<ILLMProcessorClient> _logger;
  private readonly LLMProcessorSettings _llmProcessorSettings;

  public LLMProcessorClient(
    ILogger<ILLMProcessorClient> logger,
    HttpClient httpClient,
    IOptions<LLMProcessorSettings> llmProcessorSettings) : base(logger, httpClient)
  {
    _logger = logger;
    _llmProcessorSettings = llmProcessorSettings.Value;
  }

  public async Task<Result<bool>> MatchTransactionGroup(
    List<string> transactionNames,
    List<string> existingGroups,
    string userId)
  {
    SetAuthorizationHeader(_llmProcessorSettings.Token);

    var response = await PostAsync<LLMProcessorRequestDto, LLMProcessorResponseDto>(
      _llmProcessorSettings.ApiUrl + _llmProcessorSettings.MatchTransactionEndpoint,
      new LLMProcessorRequestDto
      {
        UserId = userId,
        TransactionNames = transactionNames,
        TransactionGroupNames = existingGroups
      });

    if (response.IsSuccess)
    {
      return Result.Success(true);
    }

    _logger.LogError("Failed request to LLM Processor Service: {ErrorMessage}", response);
    return Result.Failure<bool>(response.ApplicationError!);
  }
}
