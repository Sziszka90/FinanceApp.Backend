using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public class DeleteTransactionCommandHandler : ICommandHandler<DeleteTransactionCommand, Result>
{
  private readonly IRepository<Domain.Entities.Transaction> _transactionRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<DeleteTransactionCommandHandler> _logger;

  public DeleteTransactionCommandHandler(IRepository<Domain.Entities.Transaction> transactionRepository,
                                     IUnitOfWork unitOfWork,
                                     ILogger<DeleteTransactionCommandHandler> logger)
  {
    _transactionRepository = transactionRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteTransactionCommand request, CancellationToken cancellationToken)
  {
    var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);

    if (transaction is null)
    {
      _logger.LogError("Transaction not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _transactionRepository.DeleteAsync(request.Id, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Transaction deleted with ID:{Id}", request.Id);

    return Result.Success();
  }
}
