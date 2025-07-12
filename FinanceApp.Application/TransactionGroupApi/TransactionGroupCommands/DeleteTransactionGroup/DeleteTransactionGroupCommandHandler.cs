using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.DeleteTransactionGroup;

public class DeleteTransactionGroupCommandHandler : ICommandHandler<DeleteTransactionGroupCommand, Result>
{
  private readonly ILogger<DeleteTransactionGroupCommandHandler> _logger;
  private readonly IRepository<Domain.Entities.TransactionGroup> _transactionGroupRepository;
  private readonly IUnitOfWork _unitOfWork;

  public DeleteTransactionGroupCommandHandler(
    ILogger<DeleteTransactionGroupCommandHandler> logger,
    IRepository<Domain.Entities.TransactionGroup> transactionGroupRepository,
    IUnitOfWork unitOfWork
  )
  {
    _logger = logger;
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteTransactionGroupCommand request, CancellationToken cancellationToken)
  {
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
