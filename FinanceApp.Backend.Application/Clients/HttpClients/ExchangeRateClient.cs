using FinanceApp.Backend.Application.Clients.HttpClients;
using FinanceApp.Backend.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

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

    var supportedCurrencies = Enum.GetNames<CurrencyEnum>().Where(currency => currency != nameof(CurrencyEnum.XXX));
    if (!string.Equals(response.Base, nameof(CurrencyEnum.USD), StringComparison.OrdinalIgnoreCase) ||
        response.Timestamp <= 0 || supportedCurrencies.Any(currency =>
          !response.Rates.TryGetValue(currency, out var rate) || rate <= 0))
    {
      throw new InvalidDataException("Exchange rate provider returned an incomplete or invalid USD rate set.");
    }

    var allRates = CalculateAllRatesFromUsdBase(response.Rates);

    return Result.Success(allRates);
  }

  private List<ExchangeRate> CalculateAllRatesFromUsdBase(Dictionary<string, decimal> usdRates)
  {
    var currencies = Enum.GetNames<CurrencyEnum>().Where(currency => currency != nameof(CurrencyEnum.XXX)).ToArray();
    var pairs = new List<ExchangeRate>();
    foreach (var baseCurrency in currencies)
    {
      foreach (var targetCurrency in currencies)
      {
        if (baseCurrency == targetCurrency)
        {
          continue;
        }

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
