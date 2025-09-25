using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
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
  private readonly IUserService _userService;
  private readonly IExchangeRateService _exchangeRateService;

  public GetTopTransactionGroupsQueryHandler(
    ILogger<GetTopTransactionGroupsQueryHandler> logger,
    ITransactionRepository transactionRepository,
    IUserRepository userRepository,
    IUserService userService,
    IExchangeRateService exchangeRateService)
  {
    _logger = logger;
    _transactionRepository = transactionRepository;
    _userRepository = userRepository;
    _userService = userService;
    _exchangeRateService = exchangeRateService;
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

    var allTransactions = await _transactionRepository.GetTransactionsByTopTransactionGroups(
      startDate: request.StartDate,
      endDate: request.EndDate,
      userId: user.Id,
      cancellationToken: cancellationToken
    );

    foreach (var transaction in allTransactions)
    {
      if (transaction.Value.Currency != user.BaseCurrency)
      {
        var valueInUserCurrencyResult = await _exchangeRateService.ConvertAmountAsync(
          transaction.Value.Amount,
          transaction.TransactionDate,
          transaction.Value.Currency.ToString(),
          user.BaseCurrency.ToString(),
          cancellationToken);

        if (!valueInUserCurrencyResult.IsSuccess)
        {
          _logger.LogWarning("Failed to convert amount for transaction ID {TransactionId}: {Error}",
          transaction.Id, valueInUserCurrencyResult.ApplicationError?.Message);
          continue;
        }
        transaction.Value.Amount = valueInUserCurrencyResult.Data;
        transaction.Value.Currency = user.BaseCurrency;
      }
    }

    var topGroups = allTransactions
      .GroupBy(t => t.TransactionGroup!)
      .Select(g => new TopTransactionGroupDto
      {
        Id = g.Key.Id,
        Name = g.Key.Name,
        Description = g.Key.Description,
        TransactionCount = g.Count(),
        TotalAmount = new Money
        {
          Amount = g.Sum(t => t.Value.Amount),
          Currency = user.BaseCurrency
        },
      })
      .OrderByDescending(x => x.TotalAmount.Amount)
      .Take(request.Top)
      .ToList();

    _logger.LogInformation("Retrieved top {Top} transaction groups for user {UserId} from {StartDate} to {EndDate}.", request.Top, user.Id, request.StartDate, request.EndDate);

    return Result.Success(topGroups);
  }
}
