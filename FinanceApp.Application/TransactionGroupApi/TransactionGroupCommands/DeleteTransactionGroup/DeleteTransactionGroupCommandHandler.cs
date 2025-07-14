using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.DeleteTransactionGroup;

public class DeleteTransactionGroupCommandHandler : ICommandHandler<DeleteTransactionGroupCommand, Result>
{
  private readonly ILogger<DeleteTransactionGroupCommandHandler> _logger;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IUnitOfWork _unitOfWork;

  public DeleteTransactionGroupCommandHandler(
    ILogger<DeleteTransactionGroupCommandHandler> logger,
    ITransactionGroupRepository transactionGroupRepository,
    ITransactionRepository transactionRepository,
    IUnitOfWork unitOfWork
  )
  {
    _logger = logger;
    _transactionGroupRepository = transactionGroupRepository;
    _transactionRepository = transactionRepository;
    _unitOfWork = unitOfWork;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteTransactionGroupCommand request, CancellationToken cancellationToken)
  {
    var isUsed = await _transactionRepository.TransactionGroupUsedAsync(request.Id, cancellationToken);

    if (isUsed)
    {
      _logger.LogWarning("Transaction Group with ID:{Id} cannot be deleted because it is used by transactions.", request.Id);
      return Result.Failure(ApplicationError.TransactionGroupIsUsedError());
    }

    var transactionGroup = await _transactionGroupRepository.GetByIdAsync(request.Id, noTracking: true, cancellationToken: cancellationToken);

    if (transactionGroup is null)
    {
      _logger.LogError("Transaction Group not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _transactionGroupRepository.DeleteAsync(transactionGroup, cancellationToken: cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogDebug("Transaction Group deleted with ID:{Id}", request.Id);

    return Result.Success();
  }
}
