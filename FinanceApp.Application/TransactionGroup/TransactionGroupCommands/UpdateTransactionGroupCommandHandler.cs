using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using FinanceApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupCommands;

public class UpdateTransactionGroupCommandHandler : ICommandHandler<UpdateTransactionGroupCommand, Result<GetTransactionGroupDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly ILogger<UpdateTransactionGroupCommandHandler> _logger;

  public UpdateTransactionGroupCommandHandler(IMapper mapper,
                                         IUnitOfWork unitOfWork,
                                         ITransactionGroupRepository transactionGroupRepository,
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
    var transactionGroup = await _transactionGroupRepository.GetByIdWithLimitAndIconAsync(request.UpdateTransactionGroupDto.Id, cancellationToken);

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

    Icon? groupIcon = null;

    if (request.Image is not null && request.Image.Length > 0)
    {
      using var ms = new MemoryStream();
      await request.Image.CopyToAsync(ms);
      var imageData = ms.ToArray();
      var contentType = request.Image.ContentType;
      groupIcon = new Icon(request.UpdateTransactionGroupDto.Name, contentType, imageData);
    }

    transactionGroup!.Update(request.UpdateTransactionGroupDto.Name,
                             request.UpdateTransactionGroupDto.Description,
                             groupIcon,
                             request.UpdateTransactionGroupDto.Limit);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Transaction Group updated with ID:{Id}", request.UpdateTransactionGroupDto.Id);

    return Result.Success(_mapper.Map<GetTransactionGroupDto>(await _transactionGroupRepository.UpdateAsync(transactionGroup, cancellationToken)));
  }
}
