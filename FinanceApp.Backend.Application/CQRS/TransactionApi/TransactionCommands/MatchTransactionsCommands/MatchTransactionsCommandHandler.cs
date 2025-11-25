using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Hubs;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.MatchTransactionsCommands;

public class MatchTransactionsCommandHandler : ICommandHandler<MatchTransactionsCommand, Result<bool>>
{
  private readonly ILogger<MatchTransactionsCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly IMatchTransactionRepository _matchTransactionRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ISignalRService _signalRService;

  public MatchTransactionsCommandHandler(
    ILogger<MatchTransactionsCommandHandler> logger,
    IUserRepository userRepository,
    ITransactionRepository transactionRepository,
    ITransactionGroupRepository transactionGroupRepository,
    IMatchTransactionRepository matchTransactionRepository,
    IUnitOfWork unitOfWork,
    ISignalRService signalRService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _matchTransactionRepository = matchTransactionRepository;
    _unitOfWork = unitOfWork;
    _signalRService = signalRService;
  }

  /// <inheritdoc />
  public async Task<Result<bool>> Handle(MatchTransactionsCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(new Guid(request.ResponseDto.UserId), noTracking: false, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with ID: {UserId}", request.ResponseDto.UserId);
      return Result.Failure<bool>(ApplicationError.UserNotFoundError(userId: request.ResponseDto.UserId));
    }

    var existingTransactionGroups = await _transactionGroupRepository.GetAllByUserIdAsync(user.Id, noTracking: false, cancellationToken: cancellationToken);
    var existingTransactions = await _transactionRepository.GetAllByUserIdAsync(user.Id, noTracking: false, cancellationToken: cancellationToken);

    if (existingTransactions.Count == 0)
    {
      _logger.LogWarning("No valid transactions found.");
      return Result.Failure<bool>(ApplicationError.DefaultError("Transaction list is empty."));
    }

    if (existingTransactionGroups.Count == 0)
    {
      _logger.LogWarning("No valid transaction groups found.");
      return Result.Failure<bool>(ApplicationError.DefaultError("Transaction group list is empty."));
    }

    var matchedTransactions = await _matchTransactionRepository.GetAllAsync();

    foreach (var transaction in existingTransactions)
    {
      var matchedGroup = matchedTransactions.FirstOrDefault(t => t.Transaction == transaction.Name)?.TransactionGroup;

      if (matchedGroup != null)
      {
        var group = existingTransactionGroups.FirstOrDefault(tg => tg.Name == matchedGroup);
        transaction.TransactionGroup = group;
      }
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    await _signalRService.SendToClientGroupMethodAsync(user.Email.ToString(), HubConstants.TRANSACTIONS_MATCHED_NOTIFICATION, HubConstants.REFRESH_TRANSACTIONS);

    var allTransactions = await _transactionRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);

    _logger.LogInformation("Processed matched transactions for user: {UserId}", user.Id);
    return Result.Success(true);
  }
}
