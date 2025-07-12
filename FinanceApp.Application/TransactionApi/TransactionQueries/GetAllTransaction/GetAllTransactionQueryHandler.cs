using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionApi.TransactionQueries.GetAllTransaction;

public class GetAllTransactionQueryHandler : IQueryHandler<GetAllTransactionQuery, Result<List<GetTransactionDto>>>
{
  private readonly ILogger<GetAllTransactionQueryHandler> _logger;
  private readonly IMapper _mapper;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IUserRepository _userRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public GetAllTransactionQueryHandler(
    ILogger<GetAllTransactionQueryHandler> logger,
    IMapper mapper,
    ITransactionRepository transactionRepository,
    IExchangeRateRepository exchangeRateRepository,
    IUserRepository userRepository,
    IHttpContextAccessor httpContextAccessor
  )
  {
    _logger = logger;
    _mapper = mapper;
    _transactionRepository = transactionRepository;
    _exchangeRateRepository = exchangeRateRepository;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<Result<List<GetTransactionDto>>> Handle(GetAllTransactionQuery request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    var user = await _userRepository.GetUserByEmailAsync(userEmail!, noTracking: true, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogWarning("User not found.");
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.UserNotFoundError());
    }

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(noTracking: true, cancellationToken: cancellationToken);

    if (exchangeRates is null || exchangeRates.Count == 0)
    {
      _logger.LogWarning("No exchange rates found.");
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.MissingExchangeRatesError());
    }

    List<Transaction> result;

    if (request.TransactionFilter is null)
    {
      result = await _transactionRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);
    }
    else
    {
      result = await _transactionRepository.GetAllByFilterAsync(request.TransactionFilter, noTracking: true, cancellationToken: cancellationToken);
    }

    foreach (var transaction in result)
    {
      if (transaction.Value.Currency != user.BaseCurrency)
      {

        transaction.Value.Amount = ConvertToUserCurrency(transaction.Value.Amount, transaction.Value.Currency, user.BaseCurrency, exchangeRates);
        transaction.Value.Currency = user.BaseCurrency;
      }
    }

    _logger.LogDebug("Retrieved {Count} transactions for user {UserEmail}", result.Count, userEmail);
    return Result.Success(_mapper.Map<List<GetTransactionDto>>(result));
  }

  private decimal ConvertToUserCurrency(decimal amount, CurrencyEnum fromCurrency, CurrencyEnum toCurrency, List<FinanceApp.Domain.Entities.ExchangeRate> rates)
  {
    if (fromCurrency == toCurrency)
      return Math.Round(amount, 2);
    var rate = rates.FirstOrDefault(r => r.BaseCurrency == fromCurrency.ToString() && r.TargetCurrency == toCurrency.ToString());

    return Math.Round(amount * rate!.Rate, 2);
  }
}
