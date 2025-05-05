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

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionQueries;

public class GetIncomeSumQueryHandler : IQueryHandler<GetIncomeSumQuery, Result<Money>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IExchangeRateHttpClient _exchangeRateHttpClient;
  private readonly IRepository<Domain.Entities.IncomeTransaction> _incomeTransactionRepository;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly IOptions<ExchangeRateSettings> _exchangeRateOptions;
  private readonly IHttpContextAccessor _httpContextAccessor;

  #endregion

  #region Constructors

  public GetIncomeSumQueryHandler(
    IMapper mapper,
    IRepository<Domain.Entities.IncomeTransaction> incomeTransactionRepository,
    IRepository<Domain.Entities.User> userRepository,
    IExchangeRateHttpClient exchangeRateHttpClient,
    IOptions<ExchangeRateSettings> exchangeRateOptions,
    IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _exchangeRateHttpClient = exchangeRateHttpClient;
    _incomeTransactionRepository = incomeTransactionRepository;
    _userRepository = userRepository;
    _exchangeRateOptions = exchangeRateOptions;
    _httpContextAccessor = httpContextAccessor;
  }

  #endregion

  #region Methods

  public async Task<Result<Money>> Handle(GetIncomeSumQuery request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var allIncomes = await _incomeTransactionRepository.GetAllAsync(false, cancellationToken);

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

    foreach (var income in allIncomes)
    {
      if (income.Value.Currency != targetCurrency)
      {
        var rates = await _exchangeRateHttpClient.GetDataAsync(income.Value.Currency.ToString(), targetCurrency.ToString());

        if (rates is null)
        {
          return Result.Failure<Money>(ApplicationError.DefaultError("Exchange not found"));
        }

        summAmount.Amount = summAmount.Amount + (rates[$"{income.Value.Currency.ToString()}_{targetCurrency}".ToLower()] * income.Value.Amount);
      }
      else
      {
        summAmount.Amount += income.Value.Amount;
      }
    }

    return Result.Success(summAmount);
  }

  #endregion
}