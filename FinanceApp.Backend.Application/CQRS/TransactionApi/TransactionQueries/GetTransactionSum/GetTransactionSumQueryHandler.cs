using System.Security.Claims;
using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionSum;

public class GetTransactionSumQueryHandler : IQueryHandler<GetTransactionSumQuery, Result<Money>>
{
  private readonly ILogger<GetTransactionSumQueryHandler> _logger;
  private readonly IRepository<Transaction> _transactionRepository;
  private readonly IRepository<User> _userRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public GetTransactionSumQueryHandler(
    ILogger<GetTransactionSumQueryHandler> logger,
    IRepository<Transaction> transactionRepository,
    IRepository<User> userRepository,
    IExchangeRateRepository exchangeRateRepository,
    IHttpContextAccessor httpContextAccessor)
  {
    _logger = logger;
    _transactionRepository = transactionRepository;
    _userRepository = userRepository;
    _exchangeRateRepository = exchangeRateRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<Result<Money>> Handle(GetTransactionSumQuery request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var allTransaction = await _transactionRepository.GetAllAsync(false, cancellationToken);

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var criteria = UserQueryCriteria.FindUserEmail(userEmail!);

    var user = await _userRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    if(user is null)
    {
      _logger.LogError("User not found for email: {Email}", userEmail);
      return Result.Failure<Money>(ApplicationError.UserNotFoundError(userEmail!));
    }

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(noTracking: true, cancellationToken: cancellationToken);

    var summAmount = new Money
    {
      Amount = 0,
      Currency = user[0].BaseCurrency
    };

    foreach (var transaction in allTransaction)
    {
      if (transaction.Value.Currency != user[0].BaseCurrency)
      {
        summAmount.Amount =
        summAmount.Amount + (transaction.Value.Amount * exchangeRates.Where(er => er.BaseCurrency == transaction.Value.Currency.ToString() && er.TargetCurrency == user[0].BaseCurrency.ToString()).Select(er => er.Rate).FirstOrDefault());
      }
      else
      {
        if (transaction.TransactionType == TransactionTypeEnum.Expense) summAmount.Amount -= transaction.Value.Amount;
        if (transaction.TransactionType == TransactionTypeEnum.Income) summAmount.Amount += transaction.Value.Amount;
      }
    }

    summAmount.Amount = Math.Round(summAmount.Amount, 2);

    _logger.LogInformation("Transaction sum calculated for user: {UserEmail}, Amount: {Amount} {Currency}", userEmail, summAmount.Amount, summAmount.Currency);

    return Result.Success(summAmount);
  }
}
