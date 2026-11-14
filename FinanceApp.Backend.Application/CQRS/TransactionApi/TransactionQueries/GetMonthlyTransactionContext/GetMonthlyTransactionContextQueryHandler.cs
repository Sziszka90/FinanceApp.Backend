using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetMonthlyTransactionContext;

public sealed class GetMonthlyTransactionContextQueryHandler
  : IQueryHandler<GetMonthlyTransactionContextQuery, Result<MonthlyTransactionContext>>
{
  private readonly ILogger<GetMonthlyTransactionContextQueryHandler> _logger;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IUserService _userService;
  private readonly IExchangeRateService _exchangeRateService;

  public GetMonthlyTransactionContextQueryHandler(
    ILogger<GetMonthlyTransactionContextQueryHandler> logger,
    ITransactionRepository transactionRepository,
    IUserService userService,
    IExchangeRateService exchangeRateService)
  {
    _logger = logger;
    _transactionRepository = transactionRepository;
    _userService = userService;
    _exchangeRateService = exchangeRateService;
  }

  public async Task<Result<MonthlyTransactionContext>> Handle(
    GetMonthlyTransactionContextQuery request,
    CancellationToken cancellationToken)
  {
    var activeUserResult = await _userService.GetActiveUserAsync(cancellationToken);
    if (!activeUserResult.IsSuccess || activeUserResult.Data is null)
    {
      _logger.LogWarning(
        "Could not resolve the authenticated user for monthly transaction context: {Error}",
        activeUserResult.ApplicationError?.Message);
      return Result.Failure<MonthlyTransactionContext>(activeUserResult.ApplicationError!);
    }

    var user = activeUserResult.Data;
    var monthStart = new DateTimeOffset(request.Year, request.Month, 1, 0, 0, 0, TimeSpan.Zero);
    var selectedMonthEnd = monthStart.AddMonths(1);
    var rangeStart = monthStart.AddMonths(-request.LookbackMonths);
    var transactions = await _transactionRepository.GetTransactionsByDateRangeAsync(
      user.Id,
      rangeStart,
      selectedMonthEnd,
      cancellationToken);

    var items = new List<MonthlyTransactionItem>(transactions.Count);
    foreach (var transaction in transactions)
    {
      var amountResult = await ConvertToUserCurrencyAsync(
        transaction,
        user.BaseCurrency,
        cancellationToken);

      if (!amountResult.IsSuccess)
      {
        return Result.Failure<MonthlyTransactionContext>(amountResult.ApplicationError!);
      }

      items.Add(new MonthlyTransactionItem(
        transaction.Id,
        transaction.Name,
        transaction.Description,
        transaction.TransactionDate,
        transaction.TransactionType?.ToString(),
        amountResult.Data,
        user.BaseCurrency.ToString(),
        transaction.TransactionGroup?.Id,
        transaction.TransactionGroup?.Name,
        transaction.TransactionDate >= monthStart && transaction.TransactionDate < selectedMonthEnd));
    }

    return Result.Success(new MonthlyTransactionContext(
      request.Year,
      request.Month,
      request.LookbackMonths,
      user.BaseCurrency.ToString(),
      rangeStart,
      selectedMonthEnd,
      items));
  }

  private async Task<Result<decimal>> ConvertToUserCurrencyAsync(
    Transaction transaction,
    CurrencyEnum baseCurrency,
    CancellationToken cancellationToken)
  {
    if (transaction.Value.Currency == baseCurrency)
    {
      return Result.Success(transaction.Value.Amount);
    }

    if (baseCurrency == CurrencyEnum.EUR)
    {
      return Result.Success(transaction.ValueInBaseCurrency);
    }

    var conversionResult = await _exchangeRateService.ConvertAmountAsync(
      transaction.Value.Amount,
      transaction.TransactionDate,
      transaction.Value.Currency.ToString(),
      baseCurrency.ToString(),
      cancellationToken);

    if (!conversionResult.IsSuccess)
    {
      _logger.LogWarning(
        "Could not convert transaction {TransactionId} to {Currency}: {Error}",
        transaction.Id,
        baseCurrency,
        conversionResult.ApplicationError?.Message);
      return Result.Failure<decimal>(conversionResult.ApplicationError!);
    }

    return Result.Success(conversionResult.Data);
  }
}
