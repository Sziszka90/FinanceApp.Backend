using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupCommands;

public class UpdateIncomeGroupCommandHandler : ICommandHandler<UpdateIncomeGroupCommand, Result<GetIncomeTransactionGroupDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.IncomeTransactionGroup> _incomeTransactionGroupRepository;
  private readonly ILogger<UpdateIncomeGroupCommandHandler> _logger;

  #endregion

  #region Constructors

  public UpdateIncomeGroupCommandHandler(IMapper mapper,
                                         IUnitOfWork unitOfWork,
                                         IRepository<Domain.Entities.IncomeTransactionGroup> incomeTransactionGroupRepository,
                                         ILogger<UpdateIncomeGroupCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _incomeTransactionGroupRepository = incomeTransactionGroupRepository;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<GetIncomeTransactionGroupDto>> Handle(UpdateIncomeGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _incomeTransactionGroupRepository.GetByIdAsync(request.UpdateIncomeTransactionGroupDto.Id, cancellationToken);

    if (transactionGroup is null)
    {
      _logger.LogError("Income Transaction Group not found with ID:{Id}", request.UpdateIncomeTransactionGroupDto.Id);
      return Result.Failure<GetIncomeTransactionGroupDto>(ApplicationError.EntityNotFoundError());
    }

    var transactionGroupWithSameName = await _incomeTransactionGroupRepository.GetQueryAsync(IncomeQueryCriteria.FindDuplicatedNameExludingId(request.UpdateIncomeTransactionGroupDto), cancellationToken: cancellationToken);

    if (transactionGroupWithSameName.Count > 0)
    {
      _logger.LogError("Income Transaction Group already exists with name:{Name}", request.UpdateIncomeTransactionGroupDto.Name);
      return Result.Failure<GetIncomeTransactionGroupDto>(ApplicationError.NameAlreadyExistsError(request.UpdateIncomeTransactionGroupDto.Name));
    }

    transactionGroup!.Update(request.UpdateIncomeTransactionGroupDto.Name,
                             request.UpdateIncomeTransactionGroupDto.Description,
                             request.UpdateIncomeTransactionGroupDto.Icon);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Income Transaction Group updated with ID:{Id}", request.UpdateIncomeTransactionGroupDto.Id);

    return Result.Success(_mapper.Map<GetIncomeTransactionGroupDto>(await _incomeTransactionGroupRepository.UpdateAsync(transactionGroup, cancellationToken)));
  }

  #endregion
}
