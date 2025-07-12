using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.DeleteTransactionGroup;

public class DeleteTransactionGroupCommandHandler : ICommandHandler<DeleteTransactionGroupCommand, Result>
{
  private readonly IRepository<Domain.Entities.TransactionGroup> _transactionGroupRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<DeleteTransactionGroupCommandHandler> _logger;

  public DeleteTransactionGroupCommandHandler(IRepository<Domain.Entities.TransactionGroup> transactionGroupRepository,
                                         IUnitOfWork unitOfWork,
                                         ILogger<DeleteTransactionGroupCommandHandler> logger)
  {
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteTransactionGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _transactionGroupRepository.GetByIdAsync(request.Id, cancellationToken);

    if (transactionGroup is null)
    {
      _logger.LogError("Transaction Group not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _transactionGroupRepository.DeleteAsync(request.Id, cancellationToken);

    _logger.LogInformation("Transaction Group deleted with ID:{Id}", request.Id);

    try
    {
      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error occurred while deleting Transaction Group with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.DefaultError(ex.Message));
    }
    return Result.Success();
  }
}
