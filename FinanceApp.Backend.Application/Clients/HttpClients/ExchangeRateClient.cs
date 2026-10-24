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
    return await GetRatesAsync(_exchangeRateSettings.ApiEndpoint, historical: false);
  }

  public async Task<Result<List<ExchangeRate>>> GetExchangeRatesAsync(DateOnly date)
  {
    var endpoint = _exchangeRateSettings.HistoricalEndpoint.Replace(
      "{date}", date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    return await GetRatesAsync(endpoint, historical: true);
  }

  private async Task<Result<List<ExchangeRate>>> GetRatesAsync(string endpoint, bool historical)
  {
    var response = await GetAsync<List<FrankfurterRateDto>>(endpoint);
    var first = response.FirstOrDefault();
    if (first is null ||
        !string.Equals(first.Base, nameof(CurrencyEnum.USD), StringComparison.OrdinalIgnoreCase) ||
        !DateOnly.TryParseExact(first.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
          System.Globalization.DateTimeStyles.None, out var rateDate) ||
        response.Any(row => !string.Equals(row.Base, first.Base, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(row.Date, first.Date, StringComparison.Ordinal)))
    {
      throw new InvalidDataException("Frankfurter returned an incomplete or invalid USD rate set.");
    }

    var usdRates = response.ToDictionary(row => row.Quote, row => row.Rate, StringComparer.OrdinalIgnoreCase);
    usdRates[nameof(CurrencyEnum.USD)] = 1m;
    var supportedCurrencies = Enum.GetNames<CurrencyEnum>()
      .Where(currency => currency != nameof(CurrencyEnum.XXX) && currency != nameof(CurrencyEnum.USD));
    if (supportedCurrencies.Any(currency => !usdRates.TryGetValue(currency, out var rate) || rate <= 0))
    {
      throw new InvalidDataException("Frankfurter returned an incomplete or invalid USD rate set.");
    }

    var validFrom = new DateTimeOffset(rateDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    var allRates = CalculateAllRatesFromUsdBase(usdRates, validFrom, actual: !historical);

    return Result.Success(allRates);
  }

  private List<ExchangeRate> CalculateAllRatesFromUsdBase(Dictionary<string, decimal> usdRates,
    DateTimeOffset validFrom, bool actual)
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
        pairs.Add(new ExchangeRate(baseCurrency, targetCurrency, rate)
        {
          ValidFrom = validFrom,
          Actual = actual
        });
      }
    }
    return pairs;
  }
}
