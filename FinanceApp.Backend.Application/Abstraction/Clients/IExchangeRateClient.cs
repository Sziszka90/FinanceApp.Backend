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

  /// <summary>Gets rates for the requested date, or the provider's most recent published date before it.</summary>
  Task<Result<List<ExchangeRate>>> GetExchangeRatesAsync(DateOnly date);
}
