using FinanceApp.Backend.Application.Clients.HttpClients;
using FinanceApp.Backend.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Backend.Application.Exceptions;
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
    try
    {
      return await GetRatesAsync(endpoint, historical: true);
    }
    catch (Exception exception) when (exception is HttpClientException or InvalidDataException)
    {
      // A date with no observation (weekend or holiday) can return no usable exact-date rate.
      // Query a bounded range so callers can select the latest published rate on or before it.
      return await GetHistoricalRatesInRangeAsync(date);
    }
  }

  private async Task<Result<List<ExchangeRate>>> GetHistoricalRatesInRangeAsync(DateOnly date)
  {
    var startDate = date.AddDays(-14);
    var endpoint = _exchangeRateSettings.HistoricalRangeEndpoint
      .Replace("{startDate}", startDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
      .Replace("{date}", date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    var response = await GetAsync<List<FrankfurterRateDto>>(endpoint);
    return Result.Success(ParseRateRows(response, historical: true, allowIncompleteDates: true));
  }

  private async Task<Result<List<ExchangeRate>>> GetRatesAsync(string endpoint, bool historical)
  {
    var response = await GetAsync<List<FrankfurterRateDto>>(endpoint);
    return Result.Success(ParseRateRows(response, historical));
  }

  private List<ExchangeRate> ParseRateRows(List<FrankfurterRateDto> response, bool historical,
    bool allowIncompleteDates = false)
  {
    if (response.Count == 0)
    {
      throw new InvalidDataException("Frankfurter returned an empty USD rate set.");
    }

    var supportedCurrencies = Enum.GetNames<CurrencyEnum>()
      .Where(currency => currency != nameof(CurrencyEnum.XXX) && currency != nameof(CurrencyEnum.USD))
      .ToArray();
    var allRates = new List<ExchangeRate>();
    foreach (var dateGroup in response.GroupBy(row => row.Date, StringComparer.Ordinal))
    {
      if (!DateOnly.TryParseExact(dateGroup.Key, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var rateDate) ||
          dateGroup.Any(row => !string.Equals(row.Base, nameof(CurrencyEnum.USD), StringComparison.OrdinalIgnoreCase)))
      {
        throw new InvalidDataException("Frankfurter returned an invalid USD rate set.");
      }

      var usdRates = dateGroup.ToDictionary(row => row.Quote, row => row.Rate, StringComparer.OrdinalIgnoreCase);
      usdRates[nameof(CurrencyEnum.USD)] = 1m;
      var hasAllSupportedCurrencies = supportedCurrencies.All(currency =>
        usdRates.TryGetValue(currency, out var rate) && rate > 0);
      if (!hasAllSupportedCurrencies)
      {
        if (allowIncompleteDates)
        {
          continue;
        }

        throw new InvalidDataException("Frankfurter returned an incomplete or invalid USD rate set.");
      }

      var validFrom = new DateTimeOffset(rateDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
      allRates.AddRange(CalculateAllRatesFromUsdBase(usdRates, validFrom, actual: !historical));
    }

    if (allRates.Count == 0)
    {
      throw new InvalidDataException("Frankfurter returned no complete historical USD rate set.");
    }

    return allRates;
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
