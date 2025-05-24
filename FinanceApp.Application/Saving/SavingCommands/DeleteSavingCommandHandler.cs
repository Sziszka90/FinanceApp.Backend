using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Saving.SavingCommands;

public class DeleteSavingCommandHandler : ICommandHandler<DeleteSavingCommand, Result>
{
  private readonly IRepository<Domain.Entities.Saving> _savingRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILogger<DeleteSavingCommandHandler> _logger;

  public DeleteSavingCommandHandler(IRepository<Domain.Entities.Saving> savingRepository,
                                    IUnitOfWork unitOfWork,
                                    ILogger<DeleteSavingCommandHandler> logger)
  {
    _savingRepository = savingRepository;
    _unitOfWork = unitOfWork;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(DeleteSavingCommand request, CancellationToken cancellationToken)
  {
    var saving = await _savingRepository.GetByIdAsync(request.Id, cancellationToken);

    if (saving is null)
    {
      _logger.LogError("Saving not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    await _savingRepository.DeleteAsync(request.Id, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Saving deleted with ID:{Id}", request.Id);

    return Result.Success();
  }
}
