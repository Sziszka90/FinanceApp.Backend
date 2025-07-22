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
  Task<List<ExchangeRate>> GetExchangeRatesAsync(bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a batch of exchange rates.
  /// </summary>
  /// <param name="exchangeRates"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>A list of created exchange rates</returns>
  Task<List<ExchangeRate>> BatchCreateExchangeRatesAsync(List<ExchangeRate> exchangeRates, CancellationToken cancellationToken = default);
}
