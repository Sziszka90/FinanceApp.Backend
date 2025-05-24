using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Saving.SavingCommands;

public class CreateSavingCommandHandler : ICommandHandler<CreateSavingCommand, Result<GetSavingDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.Saving> _savingRepository;
  private readonly ILogger<CreateSavingCommandHandler> _logger;

  public CreateSavingCommandHandler(IMapper mapper,
                                    IUnitOfWork unitOfWork,
                                    IRepository<Domain.Entities.Saving> savingRepository,
                                    ILogger<CreateSavingCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _savingRepository = savingRepository;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result<GetSavingDto>> Handle(CreateSavingCommand request, CancellationToken cancellationToken)
  {
    var criteria = SavingQueryCriteria.FindDuplicatedName(request.CreateSavingDto);

    var savingWithSameName = await _savingRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    if (savingWithSameName.Count > 0)
    {
      _logger.LogError("Saving already exists with name:{Name}", request.CreateSavingDto.Name);
      return Result.Failure<GetSavingDto>(ApplicationError.NameAlreadyExistsError(request.CreateSavingDto.Name));
    }

    var saving = await _savingRepository.CreateAsync(new Domain.Entities.Saving(
                                                       request.CreateSavingDto.Name,
                                                       request.CreateSavingDto.Description,
                                                       request.CreateSavingDto.Value,
                                                       request.CreateSavingDto.Type,
                                                       request.CreateSavingDto.DueDate), cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Saving created with ID:{Id}", saving.Id);

    return Result.Success(_mapper.Map<GetSavingDto>(saving));
  }
}
