using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.Abstraction.Services;

public interface IExchangeRateService
{
  /// <summary>
  /// Converts the specified amount from one currency to another based on the exchange rate
  /// </summary>
  /// <param name="fromCurrency"></param>
  /// <param name="toCurrency"></param>
  /// <param name="transactionDate"></param>
  /// <param name="amount">The amount to be converted.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>Result with exchange rate</returns>
  Task<Result<decimal>> ConvertAmountAsync(
    decimal amount,
    DateTimeOffset transactionDate,
    string fromCurrency,
    string toCurrency = "EUR",
    CancellationToken cancellationToken = default);
}
