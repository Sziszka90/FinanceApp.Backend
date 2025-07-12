using System.Text.Json;
using FinanceApp.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp.Application.Abstraction.Clients;

public class ExchangeRateClient : IExchangeRateClient
{
  private readonly ILogger<IExchangeRateClient> _logger;
  private readonly HttpClient _httpClient;
  private readonly ExchangeRateSettings _exchangeRateSettings;

  public ExchangeRateClient(
    ILogger<IExchangeRateClient> logger,
    HttpClient httpClient,
    IOptions<ExchangeRateSettings> exchangeRateOptions)
  {
    _logger = logger;
    _httpClient = httpClient;
    _exchangeRateSettings = exchangeRateOptions.Value;
  }

  public async Task<Result<List<ExchangeRate>>> GetExchangeRatesAsync(CancellationToken cancellationToken = default)
  {
    var response = await _httpClient.GetAsync(_exchangeRateSettings.ApiEndpoint + _exchangeRateSettings.AppId, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      _logger.LogError("Failed to fetch exchange rates from external service. Status code: {StatusCode}", response.StatusCode);
      return Result.Failure<List<ExchangeRate>>(ApplicationError.ExternalCallError("Exchange rate service call failed."));
    }

    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    var content = await response.Content.ReadAsStringAsync();
    var exchangeRateData = JsonSerializer.Deserialize<ExchangeRateResponseDto>(content, options);

    _logger.LogInformation("Exchange rate data fetched successfully: {Content}", content);

    if (exchangeRateData is null || exchangeRateData.Rates is null)
    {
      _logger.LogError("Invalid exchange rate response: {Content}", content);
      return Result.Failure<List<ExchangeRate>>(ApplicationError.ExternalCallError("Invalid exchange rate response."));
    }

    var allRates = CalculateAllRatesFromUsdBase(exchangeRateData.Rates);

    return Result.Success(allRates);
  }

  private List<ExchangeRate> CalculateAllRatesFromUsdBase(Dictionary<string, decimal> usdRates)
  {
    var currencies = Enum.GetNames(typeof(CurrencyEnum)).Where(c => c != nameof(CurrencyEnum.Unknown)).ToArray();
    var pairs = new List<ExchangeRate>();
    foreach (var baseCurrency in currencies)
    {
      foreach (var targetCurrency in currencies)
      {
        if (baseCurrency == targetCurrency) continue;
        decimal rate = 0;
        if (baseCurrency == CurrencyEnum.USD.ToString())
        {
          rate = usdRates.TryGetValue(targetCurrency, out var r) ? r : 0;
        }
        else if (targetCurrency == CurrencyEnum.USD.ToString())
        {
          rate = usdRates.TryGetValue(baseCurrency, out var r) && r != 0 ? 1 / r : 0;
        }
        else
        {
          if (usdRates.TryGetValue(baseCurrency, out var baseToUsd) && baseToUsd != 0 && usdRates.TryGetValue(targetCurrency, out var usdToTarget))
          {
            rate = usdToTarget / baseToUsd;
          }
        }
        pairs.Add(new ExchangeRate(baseCurrency, targetCurrency, rate));
      }
    }
    return pairs;
  }
}
