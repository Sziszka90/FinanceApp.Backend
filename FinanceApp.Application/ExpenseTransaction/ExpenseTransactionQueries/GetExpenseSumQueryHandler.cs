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

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionQueries;

public class GetExpenseSumQueryHandler : IQueryHandler<GetExpenseSumQuery, Result<Money>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IExchangeRateHttpClient _exchangeRateHttpClient;
  private readonly IRepository<Domain.Entities.ExpenseTransaction> _expenseTransactionRepository;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly IOptions<ExchangeRateSettings> _exchangeRateOptions;
  private readonly IHttpContextAccessor _httpContextAccessor;

  #endregion

  #region Constructors

  public GetExpenseSumQueryHandler(
    IMapper mapper,
    IRepository<Domain.Entities.ExpenseTransaction> expenseTransactionRepository,
    IRepository<Domain.Entities.User> userRepository,
    IExchangeRateHttpClient exchangeRateHttpClient,
    IOptions<ExchangeRateSettings> exchangeRateOptions,
    IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _exchangeRateHttpClient = exchangeRateHttpClient;
    _expenseTransactionRepository = expenseTransactionRepository;
    _userRepository = userRepository;
    _exchangeRateOptions = exchangeRateOptions;
    _httpContextAccessor = httpContextAccessor;
  }

  #endregion

  #region Methods

  public async Task<Result<Money>> Handle(GetExpenseSumQuery request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var allExpenses = await _expenseTransactionRepository.GetAllAsync(false, cancellationToken);

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

    foreach (var expense in allExpenses)
    {
      if (expense.Value.Currency != targetCurrency)
      {
        var exchangeRates = await _exchangeRateHttpClient.GetDataAsync(expense.Value.Currency.ToString(), targetCurrency.ToString());

        if (exchangeRates is null)
        {
          return Result.Failure<Money>(ApplicationError.DefaultError("Exchange not found"));
        }

        summAmount.Amount = summAmount.Amount + (expense.Value.Amount * (exchangeRates.Rates[targetCurrency.ToString()] / exchangeRates.Rates[expense.Value.Currency.ToString()]));
      }
      else
      {
        summAmount.Amount += expense.Value.Amount;
      }
    }

    return Result.Success(summAmount);
  }

  #endregion
}
