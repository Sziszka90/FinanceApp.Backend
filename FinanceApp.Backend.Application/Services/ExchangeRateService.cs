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

  public async Task<Result<ConvertedAmount>> ConvertWithRateAsync(
    decimal amount, DateTimeOffset transactionDate, string fromCurrency, string toCurrency,
    CancellationToken cancellationToken = default)
  {
    var entry = await _exchangeRateCacheManager.GetRateEntryAsync(
      transactionDate, fromCurrency, toCurrency, cancellationToken);
    if (!entry.IsSuccess)
      return Result.Failure<ConvertedAmount>(entry.ApplicationError!);

    var rate = entry.Data!;
    return Result.Success(new ConvertedAmount(Math.Round(amount * rate.Rate, 2), rate.Rate, rate.ValidFrom));
  }
}


