using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.UpdateTransactionGroup;

public class UpdateTransactionGroupCommandHandler : ICommandHandler<UpdateTransactionGroupCommand, Result<GetTransactionGroupDto>>
{
  private readonly ILogger<UpdateTransactionGroupCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly IUnitOfWork _unitOfWork;

  public UpdateTransactionGroupCommandHandler(
    ILogger<UpdateTransactionGroupCommandHandler> logger,
    IMapper mapper,
    IUnitOfWork unitOfWork,
    ITransactionGroupRepository transactionGroupRepository
  )
  {
    _logger = logger;
    _mapper = mapper;
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
  }

  /// <inheritdoc />
  public async Task<Result<GetTransactionGroupDto>> Handle(UpdateTransactionGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _transactionGroupRepository.GetByIdAsync(request.Id, noTracking: false, cancellationToken);

    if (transactionGroup is null)
    {
      _logger.LogWarning("Transaction Group not found with ID:{Id}", request.Id);
      return Result.Failure<GetTransactionGroupDto>(ApplicationError.EntityNotFoundError());
    }

    var transactionGroupWithSameName = await _transactionGroupRepository.GetQueryAsync(
      TransactionQueryCriteria.FindDuplicatedNameExludingId(request.UpdateTransactionGroupDto),
      noTracking: true,
      cancellationToken: cancellationToken);

    if (transactionGroupWithSameName.Count > 0)
    {
      _logger.LogWarning("Transaction Group already exists with name:{Name}", request.UpdateTransactionGroupDto.Name);
      return Result.Failure<GetTransactionGroupDto>(ApplicationError.NameAlreadyExistsError(request.UpdateTransactionGroupDto.Name));
    }

    transactionGroup!.Update(request.UpdateTransactionGroupDto.Name,
                             request.UpdateTransactionGroupDto.Description,
                             request.UpdateTransactionGroupDto.GroupIcon);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Transaction Group updated with ID:{Id}", request.Id);

    return Result.Success(_mapper.Map<GetTransactionGroupDto>(transactionGroup));
  }
}
