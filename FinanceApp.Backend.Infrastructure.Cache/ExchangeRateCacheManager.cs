using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace FinanceApp.Backend.Infrastructure.Cache;

public class ExchangeRateCacheManager : IExchangeRateCacheManager
{
  private readonly IDistributedCache _cache;

  public ExchangeRateCacheManager(
    IDistributedCache cache)
  {
    _cache = cache;
  }

  public async Task<Result> CacheAllRatesAsync(List<ExchangeRate> allRates, CancellationToken cancellationToken = default)
  {
    var groupedRates = allRates
    .GroupBy(r => (r.BaseCurrency, r.TargetCurrency))
    .ToDictionary(
        g => g.Key,
        g => g.OrderBy(r => r.ValidFrom).ToList()
    );

    foreach (var kvp in groupedRates)
    {
      var cacheKey = $"{kvp.Key.BaseCurrency}_{kvp.Key.TargetCurrency}";
      var serializedRates = System.Text.Json.JsonSerializer.Serialize(kvp.Value);
      var bytes = System.Text.Encoding.UTF8.GetBytes(serializedRates);

      await _cache.SetAsync(cacheKey, bytes, new DistributedCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(8)
      }, cancellationToken);
    }

    return Result.Success();
  }

  public async Task<Result<decimal>> GetRateAsync(
    DateTimeOffset transactionDate,
    string fromCurrency,
    string toCurrency,
    CancellationToken cancellationToken = default)
  {
    var cacheKey = $"{fromCurrency}_{toCurrency}";

    if (await _cache.GetAsync(cacheKey, cancellationToken) is byte[] ratesBytes)
    {
      var ratesJson = System.Text.Encoding.UTF8.GetString(ratesBytes);
      if (System.Text.Json.JsonSerializer.Deserialize<List<ExchangeRate>>(ratesJson) is List<ExchangeRate> rates)
      {
        var rateEntry = rates
            .Where(r => r.ValidFrom <= transactionDate
              && (r.ValidTo == null || r.ValidTo >= transactionDate))
            .OrderByDescending(r => r.ValidFrom)
            .FirstOrDefault();

        if (rateEntry is null)
        {
          rateEntry = rates.Where(r => r.Actual).FirstOrDefault();

          if (rateEntry is null)
          {
            return Result.Failure<decimal>(ApplicationError.MissingExchangeRatesError());
          }
          return Result.Success(rateEntry.Rate);
        }
        return Result.Success(rateEntry.Rate);
      }
    }

    return Result.Failure<decimal>(ApplicationError.MissingExchangeRatesError());
  }
}
