using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupCommands;

public class DeleteIncomeGroupCommandHandler : ICommandHandler<DeleteIncomeGroupCommand, Result>
{
  #region Members

  private readonly IRepository<Domain.Entities.IncomeTransactionGroup> _incomeTransactionGroupRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<DeleteIncomeGroupCommandHandler> _logger;

  #endregion

  #region Constructors

  public DeleteIncomeGroupCommandHandler(IRepository<Domain.Entities.IncomeTransactionGroup> incomeTransactionGroupRepository,
                                         IUnitOfWork unitOfWork,
                                         ILogger<DeleteIncomeGroupCommandHandler> logger)
  {
    _incomeTransactionGroupRepository = incomeTransactionGroupRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteIncomeGroupCommand request, CancellationToken cancellationToken)
  {
    var incomeGroup = await _incomeTransactionGroupRepository.GetByIdAsync(request.Id, cancellationToken);

    if (incomeGroup is null)
    {
      _logger.LogError("Income Transaction Group not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _incomeTransactionGroupRepository.DeleteAsync(request.Id, cancellationToken);

    _logger.LogInformation("Income Transaction Group deleted with ID:{Id}", request.Id);
    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return Result.Success();
  }

  #endregion
}
