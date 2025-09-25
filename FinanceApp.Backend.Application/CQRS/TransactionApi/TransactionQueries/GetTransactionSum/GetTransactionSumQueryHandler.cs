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
  private readonly IUserService _userService;
  private readonly IExchangeRateService _exchangeRateService;

  public GetTransactionSumQueryHandler(
    ILogger<GetTransactionSumQueryHandler> logger,
    ITransactionRepository transactionRepository,
    IUserService userService,
    IExchangeRateService exchangeRateService)
  {
    _logger = logger;
    _transactionRepository = transactionRepository;
    _userService = userService;
    _exchangeRateService = exchangeRateService;
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

    var summAmount = new Money
    {
      Amount = 0,
      Currency = user.Data!.BaseCurrency
    };

    foreach (var transaction in allTransaction)
    {
      var transactionValue = transaction.Value.Amount;

      if (transaction.Value.Currency != user.Data!.BaseCurrency)
      {
        var valueInUserCurrencyResult = await _exchangeRateService.ConvertAmountAsync(
          transaction.Value.Amount,
          transaction.TransactionDate,
          transaction.Value.Currency.ToString(),
          user.Data!.BaseCurrency.ToString(),
          cancellationToken);

        if (!valueInUserCurrencyResult.IsSuccess)
        {
          _logger.LogWarning("Failed to convert amount for transaction ID {TransactionId}: {Error}",
          transaction.Id, valueInUserCurrencyResult.ApplicationError?.Message);
          continue;
        }
        transactionValue = valueInUserCurrencyResult.Data;
      }

      if (transaction.TransactionType == TransactionTypeEnum.Expense)
      {
        summAmount.Amount -= transactionValue;
      }

      if (transaction.TransactionType == TransactionTypeEnum.Income)
      {
        summAmount.Amount += transactionValue;
      }
    }

    summAmount.Amount = Math.Round(summAmount.Amount, 2);

    _logger.LogInformation("Transaction sum calculated for user: {UserEmail}, Amount: {Amount} {Currency}", userEmail, summAmount.Amount, summAmount.Currency);

    return Result.Success(summAmount);
  }
}
