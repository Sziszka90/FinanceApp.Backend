using FinanceApp.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface IExchangeRateRepository
{
  /// <summary>
  /// Gets the exchange rate between two currencies.
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public Task<List<ExchangeRate>> GetExchangeRatesAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a batch of exchange rates.
  /// </summary>
  /// <param name="exchangeRates"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public Task<List<ExchangeRate>> CreateBatchedExchangeRatesAsync(List<ExchangeRate> exchangeRates, CancellationToken cancellationToken = default);
}
