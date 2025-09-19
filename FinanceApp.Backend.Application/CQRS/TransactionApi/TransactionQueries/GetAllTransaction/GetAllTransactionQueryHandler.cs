using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Converters;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetAllTransaction;

public class GetAllTransactionQueryHandler : IQueryHandler<GetAllTransactionQuery, Result<List<GetTransactionDto>>>
{
  private readonly ILogger<GetAllTransactionQueryHandler> _logger;
  private readonly IMapper _mapper;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IUserService _userService;

  public GetAllTransactionQueryHandler(
    ILogger<GetAllTransactionQueryHandler> logger,
    IMapper mapper,
    ITransactionRepository transactionRepository,
    IExchangeRateRepository exchangeRateRepository,
    IUserService userService
  )
  {
    _logger = logger;
    _mapper = mapper;
    _transactionRepository = transactionRepository;
    _exchangeRateRepository = exchangeRateRepository;
    _userService = userService;
  }

  public async Task<Result<List<GetTransactionDto>>> Handle(GetAllTransactionQuery request, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<List<GetTransactionDto>>(user.ApplicationError!);
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
      if (transaction.Value.Currency != user.Data!.BaseCurrency)
      {

        transaction.Value.Amount = CurrencyConverter.ConvertToUserCurrency(transaction.Value.Amount, transaction.Value.Currency, user.Data!.BaseCurrency, exchangeRates);
        transaction.Value.Currency = user.Data!.BaseCurrency;
      }
    }

    _logger.LogInformation("Retrieved {Count} transactions for user {UserEmail}", result.Count, user.Data!.Email);
    return Result.Success(_mapper.Map<List<GetTransactionDto>>(result));
  }
}
