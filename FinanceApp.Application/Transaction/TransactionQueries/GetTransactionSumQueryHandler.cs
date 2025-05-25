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
  private readonly IExchangeRateHttpClient _exchangeRateHttpClient;
  private readonly IRepository<Domain.Entities.Transaction> _transactionRepository;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly IOptions<ExchangeRateSettings> _exchangeRateOptions;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public GetTransactionSumQueryHandler(
    IMapper mapper,
    IRepository<Domain.Entities.Transaction> transactionRepository,
    IRepository<Domain.Entities.User> userRepository,
    IExchangeRateHttpClient exchangeRateHttpClient,
    IOptions<ExchangeRateSettings> exchangeRateOptions,
    IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _exchangeRateHttpClient = exchangeRateHttpClient;
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

    var summAmount = new Money
    {
      Currency = targetCurrency,
      Amount = 0
    };

    foreach (var transaction in allTransaction)
    {
      if (transaction.Value.Currency != targetCurrency)
      {
        var exchangeRates = await _exchangeRateHttpClient.GetDataAsync(transaction.Value.Currency.ToString(), targetCurrency.ToString());

        if (!exchangeRates.IsSuccess)
        {
          return Result.Failure<Money>(ApplicationError.DefaultError("Transaction request error"));
        }

        summAmount.Amount = summAmount.Amount + (transaction.Value.Amount * (exchangeRates.Data!.Rates[targetCurrency.ToString()] / exchangeRates.Data!.Rates[transaction.Value.Currency.ToString()]));
      }
      else
      {
        summAmount.Amount += transaction.Value.Amount;
      }
    }

    return Result.Success(summAmount);
  }
}
