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
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IUserRepository _userRepository;
  private readonly IUserService _userService;

  public GetTopTransactionGroupsQueryHandler(
    ILogger<GetTopTransactionGroupsQueryHandler> logger,
    ITransactionRepository transactionRepository,
    IExchangeRateRepository exchangeRateRepository,
    IUserRepository userRepository,
    IUserService userService)
  {
    _logger = logger;
    _transactionRepository = transactionRepository;
    _exchangeRateRepository = exchangeRateRepository;
    _userRepository = userRepository;
    _userService = userService;
  }

  public async Task<Result<List<TopTransactionGroupDto>>> Handle(GetTopTransactionGroupsQuery request, CancellationToken cancellationToken)
  {
    User user;

    if (request.UserId is null)
    {
      var activeUserResult = await _userService.GetActiveUserAsync(cancellationToken);
      if (!activeUserResult.IsSuccess)
      {
        _logger.LogError("Failed to retrieve active user: {Error}", activeUserResult.ApplicationError?.Message);
        return Result.Failure<List<TopTransactionGroupDto>>(activeUserResult.ApplicationError!);
      }
      user = activeUserResult.Data!;
    }
    else
    {
      var userFromRepo = await _userRepository.GetByIdAsync(Guid.Parse(request.UserId), cancellationToken: cancellationToken);
      if (userFromRepo == null)
      {
        _logger.LogError("User with ID {UserId} not found.", request.UserId);
        return Result.Failure<List<TopTransactionGroupDto>>(ApplicationError.UserNotFoundError(userId: request.UserId));
      }
      user = userFromRepo;
    }

    var aggregatedData = await _transactionRepository.GetTransactionGroupAggregatesAsync(
      userId: user.Id,
      startDate: request.StartDate,
      endDate: request.EndDate,
      topCount: request.Top,
      noTracking: true,
      cancellationToken: cancellationToken);

    if (aggregatedData.Count == 0)
    {
      _logger.LogInformation("No transaction groups found for user {UserId} in the specified date range.", user.Id);
      return Result.Success(new List<TopTransactionGroupDto>());
    }

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(noTracking: true, cancellationToken: cancellationToken);

    var groupedTransactions = aggregatedData
      .GroupBy(a => a.TransactionGroup)
      .Select(g => new
      {
        TransactionGroup = g.Key,
        TotalAmount = g.Sum(a => a.Currency == user.BaseCurrency
          ? a.TotalAmount
          : CurrencyConverter.ConvertToUserCurrency(a.TotalAmount, a.Currency, user.BaseCurrency, exchangeRates)),
        TransactionCount = g.Sum(a => a.TransactionCount)
      })
      .ToList();

    var result = groupedTransactions.Select(g => new TopTransactionGroupDto
    {
      Id = g.TransactionGroup.Id,
      Name = g.TransactionGroup.Name,
      Description = g.TransactionGroup.Description,
      GroupIcon = g.TransactionGroup.GroupIcon,
      TotalAmount = new Money { Amount = g.TotalAmount, Currency = user.BaseCurrency },
      TransactionCount = g.TransactionCount
    }).ToList();

    _logger.LogInformation("Retrieved top {Count} transaction groups for user {UserId} (optimized query)", result.Count, user.Id);
    return Result.Success(result);
  }
}
