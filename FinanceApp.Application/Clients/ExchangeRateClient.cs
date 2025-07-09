using System.Text.Json;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp.Application.Abstraction.Clients;

public class ExchangeRateClient : IExchangeRateClient
{
  private readonly HttpClient _httpClient;
  private readonly IExchangeRateRepository _exchangeRateRepository;

  private readonly ExchangeRateSettings _exchangeRateSettings;

  private readonly ILogger<ExchangeRateClient> _logger;

  public ExchangeRateClient(HttpClient httpClient, IExchangeRateRepository exchangeRateRepository, ILogger<ExchangeRateClient> logger, IOptions<ExchangeRateSettings> exchangeRateOptions)
  {
    _httpClient = httpClient;
    _exchangeRateRepository = exchangeRateRepository;
    _logger = logger;
    _exchangeRateSettings = exchangeRateOptions.Value;
  }

  public async Task<Result<List<ExchangeRate>>> GetExchangeRateAsync(CancellationToken cancellationToken = default)
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
    var exchangeRateData = System.Text.Json.JsonSerializer.Deserialize<ExchangeRateResponseDto>(content, options);

    if (exchangeRateData == null || exchangeRateData.Rates == null)
    {
      _logger.LogError("Invalid exchange rate response: {Content}", content);
      return Result.Failure<List<ExchangeRate>>(ApplicationError.ExternalCallError("Invalid exchange rate response."));
    }

    var allRates = CalculateAllRatesFromUsdBase(exchangeRateData.Rates);

    var result = await _exchangeRateRepository.CreateBatchedExchangeRatesAsync(allRates, cancellationToken);

    return Result.Success(result);

  }

  /// <summary>
  /// Given a dictionary of rates with USD as the base, calculates all rates for each currency pair (USD, EUR, GBP, HUF).
  /// </summary>
  public List<Domain.Entities.ExchangeRate> CalculateAllRatesFromUsdBase(Dictionary<string, decimal> usdRates)
  {
    var currencies = new[] { "USD", "EUR", "GBP", "HUF" };
    var pairs = new List<Domain.Entities.ExchangeRate>();
    foreach (var baseCurrency in currencies)
    {
      foreach (var targetCurrency in currencies)
      {
        if (baseCurrency == targetCurrency) continue;
        decimal rate = 0;
        if (baseCurrency == "USD")
        {
          // Direct rate from USD to target
          rate = usdRates.TryGetValue(targetCurrency, out var r) ? r : 0;
        }
        else if (targetCurrency == "USD")
        {
          // Inverse rate from base to USD
          rate = usdRates.TryGetValue(baseCurrency, out var r) && r != 0 ? 1 / r : 0;
        }
        else
        {
          // Cross rate: base -> USD -> target
          if (usdRates.TryGetValue(baseCurrency, out var baseToUsd) && baseToUsd != 0 && usdRates.TryGetValue(targetCurrency, out var usdToTarget))
          {
            rate = usdToTarget / baseToUsd;
          }
        }
        pairs.Add(new Domain.Entities.ExchangeRate
        {
          BaseCurrency = baseCurrency,
          TargetCurrency = targetCurrency,
          Rate = rate
        });
      }
    }
    return pairs;
  }
}
