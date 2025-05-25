using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupCommands;

public class UpdateTransactionGroupCommandHandler : ICommandHandler<UpdateTransactionGroupCommand, Result<GetTransactionGroupDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.TransactionGroup> _transactionGroupRepository;
  private readonly ILogger<UpdateTransactionGroupCommandHandler> _logger;

  public UpdateTransactionGroupCommandHandler(IMapper mapper,
                                         IUnitOfWork unitOfWork,
                                         IRepository<Domain.Entities.TransactionGroup> transactionGroupRepository,
                                         ILogger<UpdateTransactionGroupCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _transactionGroupRepository = transactionGroupRepository;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result<GetTransactionGroupDto>> Handle(UpdateTransactionGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _transactionGroupRepository.GetByIdAsync(request.UpdateTransactionGroupDto.Id, cancellationToken);

    if (transactionGroup is null)
    {
      _logger.LogError("Transaction Group not found with ID:{Id}", request.UpdateTransactionGroupDto.Id);
      return Result.Failure<GetTransactionGroupDto>(ApplicationError.EntityNotFoundError());
    }

    var transactionGroupWithSameName = await _transactionGroupRepository.GetQueryAsync(TransactionQueryCriteria.FindDuplicatedNameExludingId(request.UpdateTransactionGroupDto), cancellationToken: cancellationToken);

    if (transactionGroupWithSameName.Count > 0)
    {
      _logger.LogError("Transaction Group already exists with name:{Name}", request.UpdateTransactionGroupDto.Name);
      return Result.Failure<GetTransactionGroupDto>(ApplicationError.NameAlreadyExistsError(request.UpdateTransactionGroupDto.Name));
    }

    transactionGroup!.Update(request.UpdateTransactionGroupDto.Name,
                             request.UpdateTransactionGroupDto.Description,
                             request.UpdateTransactionGroupDto.GroupIcon,
                             request.UpdateTransactionGroupDto.Limit);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Transaction Group updated with ID:{Id}", request.UpdateTransactionGroupDto.Id);

    return Result.Success(_mapper.Map<GetTransactionGroupDto>(await _transactionGroupRepository.UpdateAsync(transactionGroup, cancellationToken)));
  }
}
