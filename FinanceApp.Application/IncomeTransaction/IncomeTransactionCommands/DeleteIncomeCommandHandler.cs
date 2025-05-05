using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class DeleteIncomeCommandHandler : ICommandHandler<DeleteIncomeCommand, Result>
{
  #region Members

  private readonly IRepository<Domain.Entities.IncomeTransaction> _incomeTransactionRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<DeleteIncomeCommandHandler> _logger;

  #endregion

  #region Constructors

  public DeleteIncomeCommandHandler(IRepository<Domain.Entities.IncomeTransaction> incomeTransactionRepository,
                                    IUnitOfWork unitOfWork,
                                    ILogger<DeleteIncomeCommandHandler> logger)
  {
    _incomeTransactionRepository = incomeTransactionRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteIncomeCommand request, CancellationToken cancellationToken)
  {
    var expense = await _incomeTransactionRepository.GetByIdAsync(request.Id, cancellationToken);

    if (expense is null)
    {
      _logger.LogError("Income Transaction not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _incomeTransactionRepository.DeleteAsync(request.Id, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Income Transaction deleted with ID:{Id}", request.Id);

    return Result.Success();
  }

  #endregion
}