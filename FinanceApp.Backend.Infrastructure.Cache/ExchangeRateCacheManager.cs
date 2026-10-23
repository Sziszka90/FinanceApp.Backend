using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Infrastructure.Cache;

public class ExchangeRateCacheManager : IExchangeRateCacheManager
{
  private readonly IDistributedCache _cache;
  private readonly IServiceScopeFactory _scopeFactory;
  private readonly ILogger<ExchangeRateCacheManager> _logger;
  private volatile bool _cacheReady;

  public ExchangeRateCacheManager(
    IDistributedCache cache,
    IServiceScopeFactory scopeFactory,
    ILogger<ExchangeRateCacheManager> logger)
  {
    _cache = cache;
    _scopeFactory = scopeFactory;
    _logger = logger;
  }

  public async Task<Result> CacheAllRatesAsync(List<ExchangeRate> allRates, CancellationToken cancellationToken = default)
  {
    _cacheReady = false;
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

    _cacheReady = true;
    return Result.Success();
  }

  public async Task<Result<decimal>> GetRateAsync(
    DateTimeOffset transactionDate,
    string fromCurrency,
    string toCurrency,
    CancellationToken cancellationToken = default)
  {
    var entry = await GetRateEntryAsync(transactionDate, fromCurrency, toCurrency, cancellationToken);
    return entry.IsSuccess
      ? Result.Success(entry.Data!.Rate)
      : Result.Failure<decimal>(entry.ApplicationError!);
  }

  public async Task<Result<ExchangeRate>> GetRateEntryAsync(
    DateTimeOffset transactionDate,
    string fromCurrency,
    string toCurrency,
    CancellationToken cancellationToken = default)
  {
    var cacheKey = $"{fromCurrency}_{toCurrency}";
    try
    {
      if (_cacheReady && await _cache.GetAsync(cacheKey, cancellationToken) is byte[] ratesBytes)
      {
        var rates = System.Text.Json.JsonSerializer.Deserialize<List<ExchangeRate>>(ratesBytes);
        var cached = SelectRate(rates, transactionDate, fromCurrency, toCurrency);
        if (cached is not null) return Result.Success(cached);
      }
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      _logger.LogWarning(ex, "Exchange rate cache unavailable; reading saved rates from database");
    }

    using var scope = _scopeFactory.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();
    var allRates = await repository.GetExchangeRatesAsync(noTracking: true, cancellationToken: cancellationToken);
    var saved = SelectRate(allRates, transactionDate, fromCurrency, toCurrency);
    if (saved is null)
      return Result.Failure<ExchangeRate>(ApplicationError.MissingExchangeRatesError());

    try
    {
      await CacheAllRatesAsync(allRates, cancellationToken);
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      _logger.LogWarning(ex, "Could not refresh the exchange rate cache");
    }
    return Result.Success(saved);
  }

  private static ExchangeRate? SelectRate(IEnumerable<ExchangeRate>? rates, DateTimeOffset transactionDate,
    string fromCurrency, string toCurrency)
  {
    var postingDay = DateOnly.FromDateTime(transactionDate.Date);
    return rates?
      .Where(rate => rate.BaseCurrency == fromCurrency && rate.TargetCurrency == toCurrency && rate.Rate > 0 &&
                     DateOnly.FromDateTime(rate.ValidFrom.UtcDateTime) <= postingDay)
      .OrderByDescending(rate => rate.ValidFrom)
      .FirstOrDefault();
  }
}
