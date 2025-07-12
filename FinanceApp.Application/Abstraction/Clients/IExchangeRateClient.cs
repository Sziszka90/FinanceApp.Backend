using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Clients;

public interface IExchangeRateClient
{
  /// <summary>
  /// Gets the exchange rate for a specific currency.
  /// </summary>
  /// <returns>The exchange rates</returns>
  Task<Result<List<ExchangeRate>>> GetExchangeRatesAsync();
}
