using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.DeleteTransaction;

public class DeleteTransactionCommandHandler : ICommandHandler<DeleteTransactionCommand, Result>
{
  private readonly ILogger<DeleteTransactionCommandHandler> _logger;
  private readonly IRepository<Domain.Entities.Transaction> _transactionRepository;
  private readonly IUnitOfWork _unitOfWork;

  public DeleteTransactionCommandHandler(
    ILogger<DeleteTransactionCommandHandler> logger,
    IRepository<Domain.Entities.Transaction> transactionRepository,
    IUnitOfWork unitOfWork)
  {
    _logger = logger;
    _transactionRepository = transactionRepository;
    _unitOfWork = unitOfWork;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
  {
    var transaction = await _transactionRepository.GetByIdAsync(request.Id, noTracking: true, cancellationToken: cancellationToken);

    if (transaction is null)
    {
      _logger.LogError("Transaction not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _transactionRepository.DeleteAsync(transaction, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogDebug("Transaction deleted with ID:{Id}", request.Id);

    return Result.Success();
  }
}
