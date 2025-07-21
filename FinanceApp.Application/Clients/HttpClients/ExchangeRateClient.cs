using FinanceApp.Application.Clients.HttpClients;
using FinanceApp.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using FinanceApp.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp.Application.Abstraction.Clients;

public class ExchangeRateClient : HttpClientBase<IExchangeRateClient>, IExchangeRateClient
{
  private readonly ExchangeRateSettings _exchangeRateSettings;

  public ExchangeRateClient(
    ILogger<IExchangeRateClient> logger,
    HttpClient httpClient,
    IOptions<ExchangeRateSettings> exchangeRateOptions) : base(logger, httpClient)
  {
    _exchangeRateSettings = exchangeRateOptions.Value;
  }

  public async Task<Result<List<ExchangeRate>>> GetExchangeRatesAsync()
  {
    var response = await GetAsync<ExchangeRateResponseDto>(_exchangeRateSettings.ApiEndpoint + _exchangeRateSettings.AppId);

    if (!response.IsSuccess)
    {
      return Result.Failure<List<ExchangeRate>>(response.ApplicationError!);
    }

    var allRates = CalculateAllRatesFromUsdBase(response.Data!.Rates);

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
