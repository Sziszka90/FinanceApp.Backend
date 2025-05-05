using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Saving.SavingCommands;

public class UpdateSavingCommandHandler : ICommandHandler<UpdateSavingCommand, Result<GetSavingDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.Saving> _savingRepository;
  private readonly ILogger<UpdateSavingCommandHandler> _logger;

  #endregion

  #region Constructors

  public UpdateSavingCommandHandler(IMapper mapper,
                                    IUnitOfWork unitOfWork,
                                    IRepository<Domain.Entities.Saving> savingRepository,
                                    ILogger<UpdateSavingCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _savingRepository = savingRepository;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<GetSavingDto>> Handle(UpdateSavingCommand request, CancellationToken cancellationToken)
  {
    var savingWithSameName = await _savingRepository.GetQueryAsync(SavingQueryCriteria.FindDuplicatedNameExludingId(request.UpdateSavingDto), cancellationToken: cancellationToken);

    if (savingWithSameName.Count > 0)
    {
      _logger.LogError("Saving already exists with name:{Name}", request.UpdateSavingDto.Name);
      return Result.Failure<GetSavingDto>(ApplicationError.NameAlreadyExistsError(request.UpdateSavingDto.Name));
    }

    var saving = await _savingRepository.GetByIdAsync(request.UpdateSavingDto.Id, cancellationToken);

    if (saving is null)
    {
      _logger.LogError("Saving not found with ID:{Id}", request.UpdateSavingDto.Id);
      return Result.Failure<GetSavingDto>(ApplicationError.EntityNotFoundError());
    }

    saving.Update(
      request.UpdateSavingDto.Name,
      request.UpdateSavingDto.Description,
      request.UpdateSavingDto.Amount,
      request.UpdateSavingDto.Type,
      request.UpdateSavingDto.DueDate
    );

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Saving updated with ID:{Id}", request.UpdateSavingDto.Id);

    return Result.Success(_mapper.Map<GetSavingDto>(await _savingRepository.UpdateAsync(saving, cancellationToken)));
  }

  #endregion
}