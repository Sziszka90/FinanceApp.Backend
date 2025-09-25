using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.Services;

public class ExchangeRateService : IExchangeRateService
{
  private readonly IExchangeRateCacheManager _exchangeRateCacheManager;

  public ExchangeRateService(IExchangeRateCacheManager exchangeRateCacheManager)
  {
    _exchangeRateCacheManager = exchangeRateCacheManager;
  }

  public async Task<Result<decimal>> ConvertAmountAsync(
    decimal amount,
    DateTimeOffset transactionDate,
    string fromCurrency,
    string toCurrency,
    CancellationToken cancellationToken = default)
  {
    var exchangeRate = await _exchangeRateCacheManager.GetRateAsync(transactionDate, fromCurrency, toCurrency, cancellationToken);

    if (exchangeRate.IsSuccess)
    {
      return Result.Success(Math.Round(amount * exchangeRate.Data, 2));
    }
    return Result.Failure<decimal>(exchangeRate.ApplicationError!);
  }
}


