using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;

public class DeleteExpenseCommandHandler : ICommandHandler<DeleteExpenseCommand, Result>
{
  #region Members

  private readonly IRepository<Domain.Entities.ExpenseTransaction> _expenseTransactionRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<DeleteExpenseCommandHandler> _logger;

  #endregion

  #region Constructors

  public DeleteExpenseCommandHandler(IRepository<Domain.Entities.ExpenseTransaction> expenseTransactionRepository,
                                     IUnitOfWork unitOfWork,
                                     ILogger<DeleteExpenseCommandHandler> logger)
  {
    _expenseTransactionRepository = expenseTransactionRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
  {
    var expense = await _expenseTransactionRepository.GetByIdAsync(request.Id, cancellationToken);

    if (expense is null)
    {
      _logger.LogError("Expense Transaction not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _expenseTransactionRepository.DeleteAsync(request.Id, cancellationToken);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Expense Transaction deleted with ID:{Id}", request.Id);

    return Result.Success();
  }

  #endregion
}
