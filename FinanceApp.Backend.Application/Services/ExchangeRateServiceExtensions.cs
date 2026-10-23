using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.Services;

public static class ExchangeRateServiceExtensions
{
  // The fallback preserves existing IExchangeRateService implementations while the
  // application's concrete service also records the selected saved snapshot.
  public static async Task<Result<ConvertedAmount>> ConvertWithRateAsync(this IExchangeRateService service,
    decimal amount, DateTimeOffset transactionDate, string fromCurrency, string toCurrency,
    CancellationToken cancellationToken = default)
  {
    if (service is ExchangeRateService exchangeRateService)
      return await exchangeRateService.ConvertWithRateAsync(
        amount, transactionDate, fromCurrency, toCurrency, cancellationToken);

    var converted = await service.ConvertAmountAsync(amount, transactionDate, fromCurrency, toCurrency, cancellationToken);
    return converted.IsSuccess
      ? Result.Success(new ConvertedAmount(converted.Data, null, null))
      : Result.Failure<ConvertedAmount>(converted.ApplicationError!);
  }
}
