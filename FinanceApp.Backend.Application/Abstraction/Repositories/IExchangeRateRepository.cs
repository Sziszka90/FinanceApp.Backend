using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Abstraction.Repositories;

public interface IExchangeRateRepository : IRepository<ExchangeRate>
{
  /// <summary>
  /// Gets the exchange rate between two currencies.
  /// </summary>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken"></param>
  /// <returns>A list of existing exchange rates</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error retrieving exchange rates.</exception>
  Task<List<ExchangeRate>> GetExchangeRatesAsync(bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the actual exchange rates.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns>A list of actual exchange rates</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error retrieving actual exchange rates
  Task<List<ExchangeRate>> GetActualExchangeRatesAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the exchange rate by date.
  /// </summary>
  /// <param name="validFrom"></param>
  /// <param name="validTo"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>A list of actual exchange rates</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error retrieving actual exchange rates
  Task<List<ExchangeRate>> GetExchangeRatesByDateRangeAsync(DateTimeOffset date, CancellationToken cancellationToken = default);


  /// <summary>
  /// Creates a batch of exchange rates.
  /// </summary>
  /// <param name="exchangeRates"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>A list of created exchange rates</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error creating exchange rates.</exception>
  Task<List<ExchangeRate>> BatchCreateExchangeRatesAsync(List<ExchangeRate> exchangeRates, CancellationToken cancellationToken = default);
}
