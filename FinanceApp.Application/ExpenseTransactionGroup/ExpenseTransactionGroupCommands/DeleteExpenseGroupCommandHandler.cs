using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;

public class DeleteExpenseGroupCommandHandler : ICommandHandler<DeleteExpenseGroupCommand, Result>
{
  #region Members

  private readonly IRepository<Domain.Entities.ExpenseTransactionGroup> _expenseTransactionGroupRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<DeleteExpenseGroupCommandHandler> _logger;

  #endregion

  #region Constructors

  public DeleteExpenseGroupCommandHandler(IRepository<Domain.Entities.ExpenseTransactionGroup> expenseTransactionGroupRepository,
                                          IUnitOfWork unitOfWork,
                                          ILogger<DeleteExpenseGroupCommandHandler> logger)
  {
    _expenseTransactionGroupRepository = expenseTransactionGroupRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteExpenseGroupCommand request, CancellationToken cancellationToken)
  {
    var expenseGroup = await _expenseTransactionGroupRepository.GetByIdAsync(request.Id, cancellationToken);

    if (expenseGroup is null)
    {
      _logger.LogError("Expense Transaction Group not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _expenseTransactionGroupRepository.DeleteAsync(request.Id, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Expense Transaction Group deleted with id: {Id}", request.Id);

    return Result.Success();
  }

  #endregion
}