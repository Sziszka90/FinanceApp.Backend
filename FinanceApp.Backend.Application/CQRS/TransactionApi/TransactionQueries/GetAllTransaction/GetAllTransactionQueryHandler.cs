using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
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
  private readonly IUserService _userService;
  private readonly IExchangeRateService _exchangeRateService;

  public GetAllTransactionQueryHandler(
    ILogger<GetAllTransactionQueryHandler> logger,
    IMapper mapper,
    ITransactionRepository transactionRepository,
    IUserService userService,
    IExchangeRateService exchangeRateService
  )
  {
    _logger = logger;
    _mapper = mapper;
    _transactionRepository = transactionRepository;
    _userService = userService;
    _exchangeRateService = exchangeRateService;
  }

  public async Task<Result<List<GetTransactionDto>>> Handle(GetAllTransactionQuery request, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<List<GetTransactionDto>>(user.ApplicationError!);
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
        transaction.Value.Currency = user.Data!.BaseCurrency;

        var valueInUserCurrencyResult = await _exchangeRateService.ConvertAmountAsync(
          transaction.Value.Amount,
          transaction.TransactionDate,
          transaction.Value.Currency.ToString(),
          user.Data!.BaseCurrency.ToString(),
          cancellationToken);

        if (!valueInUserCurrencyResult.IsSuccess)
        {
          _logger.LogWarning("Failed to convert amount for transaction ID {TransactionId}: {Error}", transaction.Id, valueInUserCurrencyResult.ApplicationError?.Message);
          continue;
        }

        transaction.Value.Amount = valueInUserCurrencyResult.Data;
        _logger.LogInformation("Converted transaction ID {TransactionId} to user's base currency {BaseCurrency}", transaction.Id, user.Data!.BaseCurrency);
      }
    }

    _logger.LogInformation("Retrieved {Count} transactions for user {UserEmail}", result.Count, user.Data!.Email);
    return Result.Success(_mapper.Map<List<GetTransactionDto>>(result));
  }
}
