using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Converters;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetTopTransactionGroups;

public class GetTopTransactionGroupsQueryHandler : IQueryHandler<GetTopTransactionGroupsQuery, Result<List<TopTransactionGroupDto>>>
{
  private readonly ILogger<GetTopTransactionGroupsQueryHandler> _logger;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IUserRepository _userRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IUserService _userService;

  public GetTopTransactionGroupsQueryHandler(
    ILogger<GetTopTransactionGroupsQueryHandler> logger,
    ITransactionRepository transactionRepository,
    IUserRepository userRepository,
    IExchangeRateRepository exchangeRateRepository,
    IUserService userService)
  {
    _logger = logger;
    _transactionRepository = transactionRepository;
    _userRepository = userRepository;
    _exchangeRateRepository = exchangeRateRepository;
    _userService = userService;
  }

  public async Task<Result<List<TopTransactionGroupDto>>> Handle(GetTopTransactionGroupsQuery request, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<List<TopTransactionGroupDto>>(user.ApplicationError!);
    }

    var aggregatedData = await _transactionRepository.GetTransactionGroupAggregatesAsync(
      userId: user.Data!.Id,
      startDate: request.StartDate,
      endDate: request.EndDate,
      topCount: request.Top,
      noTracking: true,
      cancellationToken: cancellationToken);

    if (aggregatedData.Count == 0)
    {
      _logger.LogInformation("No transaction groups found for user {UserId} in the specified date range.", user.Data!.Id);
      return Result.Success(new List<TopTransactionGroupDto>());
    }

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(noTracking: true, cancellationToken: cancellationToken);

    var groupedTransactions = aggregatedData
      .GroupBy(a => a.TransactionGroup)
      .Select(g => new
      {
        TransactionGroup = g.Key,
        TotalAmount = g.Sum(a => a.Currency == user.Data!.BaseCurrency
          ? a.TotalAmount
          : CurrencyConverter.ConvertToUserCurrency(a.TotalAmount, a.Currency, user.Data!.BaseCurrency, exchangeRates)),
        TransactionCount = g.Sum(a => a.TransactionCount)
      })
      .ToList();

    var result = groupedTransactions.Select(g => new TopTransactionGroupDto
    {
      Id = g.TransactionGroup.Id,
      Name = g.TransactionGroup.Name,
      Description = g.TransactionGroup.Description,
      GroupIcon = g.TransactionGroup.GroupIcon,
      TotalAmount = new Money { Amount = g.TotalAmount, Currency = user.Data!.BaseCurrency },
      TransactionCount = g.TransactionCount
    }).ToList();

    _logger.LogInformation("Retrieved top {Count} transaction groups for user {UserId} (optimized query)", result.Count, user.Data!.Id);
    return Result.Success(result);
  }
}
