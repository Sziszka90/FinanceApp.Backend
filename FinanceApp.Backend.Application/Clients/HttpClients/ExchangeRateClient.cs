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
    return Result.Success(ParseRateRows(response, historical: true));
  }

  private async Task<Result<List<ExchangeRate>>> GetRatesAsync(string endpoint, bool historical)
  {
    var response = await GetAsync<List<FrankfurterRateDto>>(endpoint);
    return Result.Success(ParseRateRows(response, historical));
  }

  private List<ExchangeRate> ParseRateRows(List<FrankfurterRateDto> response, bool historical)
  {
    if (response.Count == 0)
    {
      throw new InvalidDataException("Frankfurter returned an empty USD rate set.");
    }

    var supportedCurrencies = Enum.GetNames<CurrencyEnum>()
      .Where(currency => currency != nameof(CurrencyEnum.XXX) && currency != nameof(CurrencyEnum.USD))
      .ToHashSet(StringComparer.OrdinalIgnoreCase);
    var usdRates = new Dictionary<string, (decimal Rate, DateOnly Date)>(StringComparer.OrdinalIgnoreCase);
    foreach (var row in response)
    {
      if (!DateOnly.TryParseExact(row.Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var rateDate) ||
          !string.Equals(row.Base, nameof(CurrencyEnum.USD), StringComparison.OrdinalIgnoreCase) || row.Rate <= 0)
      {
        throw new InvalidDataException("Frankfurter returned an invalid USD rate set.");
      }

      if (supportedCurrencies.Contains(row.Quote) &&
          (!usdRates.TryGetValue(row.Quote, out var existingRate) || rateDate > existingRate.Date))
      {
        usdRates[row.Quote] = (row.Rate, rateDate);
      }
    }

    if (supportedCurrencies.Any(currency => !usdRates.ContainsKey(currency)))
    {
      throw new InvalidDataException("Frankfurter returned an incomplete or invalid USD rate set.");
    }

    return CalculateAllRatesFromUsdBase(usdRates, actual: !historical);
  }

  private List<ExchangeRate> CalculateAllRatesFromUsdBase(
    Dictionary<string, (decimal Rate, DateOnly Date)> usdRates, bool actual)
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

        decimal rate;
        DateOnly validFrom;
        if (baseCurrency == CurrencyEnum.USD.ToString())
        {
          var targetRate = usdRates[targetCurrency];
          rate = targetRate.Rate;
          validFrom = targetRate.Date;
        }
        else if (targetCurrency == CurrencyEnum.USD.ToString())
        {
          var baseRate = usdRates[baseCurrency];
          rate = 1 / baseRate.Rate;
          validFrom = baseRate.Date;
        }
        else
        {
          var baseRate = usdRates[baseCurrency];
          var targetRate = usdRates[targetCurrency];
          rate = targetRate.Rate / baseRate.Rate;
          validFrom = baseRate.Date < targetRate.Date ? baseRate.Date : targetRate.Date;
        }

        pairs.Add(new ExchangeRate(baseCurrency, targetCurrency, rate)
        {
          ValidFrom = new DateTimeOffset(validFrom.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
          Actual = actual
        });
      }
    }
    return pairs;
  }
}
