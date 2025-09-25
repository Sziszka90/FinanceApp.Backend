using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface IExchangeRateCacheManager
{
  /// <summary>
  /// Caches all exchange rates from the repository into the distributed cache.
  /// </summary>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation, containing a Result indicating success or failure
  Task<Result> CacheAllRatesAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves the exchange rate for the specified currency pair and transaction date.
  /// If the rates are not found in the cache, it attempts to refresh the cache from
  /// the repository.
  /// </summary>
  /// <param name="transactionDate">The date of the transaction for which the rate is needed.</param>
  /// <param name="fromCurrency">The base currency code (e.g., "USD").</param>
  /// <param name="toCurrency">The target currency code (e.g., "USD").</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>Result containing the exchange rate if found; otherwise, an error result.</returns>
  Task<Result<decimal>> GetRateAsync(DateTimeOffset transactionDate, string fromCurrency, string toCurrency, CancellationToken cancellationToken = default);
}
