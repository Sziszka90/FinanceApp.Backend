using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FinanceApp.Application.TransactionApi.TransactionQueries.GetTransactionSum;

public class GetTransactionSumQueryHandler : IQueryHandler<GetTransactionSumQuery, Result<Money>>
{
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.Transaction> _transactionRepository;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;

  private readonly IExchangeRateRepository _exchangeRateRepository;

  public GetTransactionSumQueryHandler(
    IMapper mapper,
    IRepository<Domain.Entities.Transaction> transactionRepository,
    IRepository<Domain.Entities.User> userRepository,
    IExchangeRateRepository exchangeRateRepository,
    IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _transactionRepository = transactionRepository;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
    _exchangeRateRepository = exchangeRateRepository;
  }

  public async Task<Result<Money>> Handle(GetTransactionSumQuery request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var allTransaction = await _transactionRepository.GetAllAsync(false, cancellationToken);

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    var criteria = UserQueryCriteria.FindUserEmail(userEmail!);

    var user = await _userRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(cancellationToken);

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

    return Result.Success(summAmount);
  }
}
