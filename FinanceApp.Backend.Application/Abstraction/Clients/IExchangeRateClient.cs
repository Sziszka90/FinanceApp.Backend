using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface IExchangeRateClient
{
  /// <summary>
  /// Gets the exchange rate for a specific currency.
  /// </summary>
  /// <returns>The exchange rates</returns>
  Task<Result<List<ExchangeRate>>> GetExchangeRatesAsync();
}
