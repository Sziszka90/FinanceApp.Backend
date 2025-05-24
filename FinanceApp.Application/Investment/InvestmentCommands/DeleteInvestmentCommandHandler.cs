using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Investment.InvestmentCommands;

public class DeleteInvestmentCommandHandler : ICommandHandler<DeleteInvestmentCommand, Result>
{
  #region Members

  private readonly IRepository<Domain.Entities.Investment> _investmentRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<DeleteInvestmentCommandHandler> _logger;

  #endregion

  #region Constructors

  public DeleteInvestmentCommandHandler(IRepository<Domain.Entities.Investment> investmentRepository,
                                        IUnitOfWork unitOfWork,
                                        ILogger<DeleteInvestmentCommandHandler> logger)
  {
    _investmentRepository = investmentRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteInvestmentCommand request, CancellationToken cancellationToken)
  {
    var investment = await _investmentRepository.GetByIdAsync(request.Id, cancellationToken);

    if (investment is null)
    {
      _logger.LogError("Investment not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _investmentRepository.DeleteAsync(request.Id, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Investment deleted with ID:{Id}", request.Id);

    return Result.Success();
  }

  #endregion
}
