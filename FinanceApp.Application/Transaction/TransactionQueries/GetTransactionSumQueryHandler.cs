using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.HttpClients;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FinanceApp.Application.Transaction.TransactionQueries;

public class GetTransactionSumQueryHandler : IQueryHandler<GetTransactionSumQuery, Result<Money>>
{
  private readonly IMapper _mapper;
  private readonly ILLMClient _llmClient;
  private readonly IRepository<Domain.Entities.Transaction> _transactionRepository;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly IOptions<ExchangeRateSettings> _exchangeRateOptions;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public GetTransactionSumQueryHandler(
    IMapper mapper,
    IRepository<Domain.Entities.Transaction> transactionRepository,
    IRepository<Domain.Entities.User> userRepository,
    ILLMClient llmClient,
    IOptions<ExchangeRateSettings> exchangeRateOptions,
    IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _llmClient = llmClient;
    _transactionRepository = transactionRepository;
    _userRepository = userRepository;
    _exchangeRateOptions = exchangeRateOptions;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<Result<Money>> Handle(GetTransactionSumQuery request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var allTransaction = await _transactionRepository.GetAllAsync(false, cancellationToken);

    var currentUserName = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    var criteria = UserQueryCriteria.FindUserName(currentUserName!);

    var user = await _userRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    var baseCurrency = user[0]
                       .BaseCurrency.ToString();

    var targetCurrency = (CurrencyEnum)Enum.Parse(typeof(CurrencyEnum), baseCurrency);

    var exchangeRates = await _llmClient.GetExchangeDataAsync(targetCurrency);

    if (!exchangeRates.IsSuccess)
    {
      return Result.Failure<Money>(exchangeRates.ApplicationError!);
    }

    var summAmount = new Money
    {
      Amount = 0,
      Currency = targetCurrency
    };

    foreach (var transaction in allTransaction)
    {
      if (transaction.Value.Currency != targetCurrency)
      {
        summAmount.Amount = summAmount.Amount + (transaction.Value.Amount * (exchangeRates.Data!.Rates[targetCurrency.ToString()] / exchangeRates.Data!.Rates[transaction.Value.Currency.ToString()]));
      }
      else
      {
        if (transaction.TransactionType == TransactionTypeEnum.Expense) summAmount.Amount -= transaction.Value.Amount;
        if (transaction.TransactionType == TransactionTypeEnum.Income) summAmount.Amount += transaction.Value.Amount;
      }
    }

    return Result.Success(summAmount);
  }
}
