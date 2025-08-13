using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionSum;

public class GetTransactionSumQueryHandler : IQueryHandler<GetTransactionSumQuery, Result<Money>>
{
  private readonly ILogger<GetTransactionSumQueryHandler> _logger;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IUserService _userService;

  public GetTransactionSumQueryHandler(
    ILogger<GetTransactionSumQueryHandler> logger,
    ITransactionRepository transactionRepository,
    IExchangeRateRepository exchangeRateRepository,
    IUserService userService)
  {
    _logger = logger;
    _transactionRepository = transactionRepository;
    _exchangeRateRepository = exchangeRateRepository;
    _userService = userService;
  }

  public async Task<Result<Money>> Handle(GetTransactionSumQuery request, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<Money>(user.ApplicationError!);
    }

    var allTransaction = await _transactionRepository.GetAllAsync(false, cancellationToken);

    var userEmail = user.Data!.Email;

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(noTracking: true, cancellationToken: cancellationToken);

    var summAmount = new Money
    {
      Amount = 0,
      Currency = user.Data!.BaseCurrency
    };

    foreach (var transaction in allTransaction)
    {
      if (transaction.Value.Currency != user.Data!.BaseCurrency)
      {
        summAmount.Amount =
        summAmount.Amount + (transaction.Value.Amount * exchangeRates.Where(er => er.BaseCurrency == transaction.Value.Currency.ToString() && er.TargetCurrency == user.Data!.BaseCurrency.ToString()).Select(er => er.Rate).FirstOrDefault());
      }
      else
      {
        if (transaction.TransactionType == TransactionTypeEnum.Expense)
        {
          summAmount.Amount -= transaction.Value.Amount;
        }

        if (transaction.TransactionType == TransactionTypeEnum.Income)
        {
          summAmount.Amount += transaction.Value.Amount;
        }
      }
    }

    summAmount.Amount = Math.Round(summAmount.Amount, 2);

    _logger.LogInformation("Transaction sum calculated for user: {UserEmail}, Amount: {Amount} {Currency}", userEmail, summAmount.Amount, summAmount.Currency);

    return Result.Success(summAmount);
  }
}
