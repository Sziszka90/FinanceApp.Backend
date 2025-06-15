using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public class UpdateTransactionCommandHandler : ICommandHandler<UpdateTransactionCommand, Result<GetTransactionDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.Transaction> _transactionRepository;
  private readonly IRepository<Domain.Entities.TransactionGroup> _transactionGroupRepository;
  private readonly ILogger<UpdateTransactionCommandHandler> _logger;

  public UpdateTransactionCommandHandler(IMapper mapper,
                                     IUnitOfWork unitOfWork,
                                     IRepository<Domain.Entities.Transaction> transactionRepository,
                                     IRepository<Domain.Entities.TransactionGroup> transactionGroupRepository,
                                     ILogger<UpdateTransactionCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result<GetTransactionDto>> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
  {
    Domain.Entities.TransactionGroup? transactionGroup = null;

    if (request.UpdateTransactionDto.TransactionGroupId is not null)
    {
      transactionGroup = await _transactionGroupRepository.GetByIdAsync((Guid)request.UpdateTransactionDto.TransactionGroupId, cancellationToken);

      if (transactionGroup is null)
      {
        _logger.LogError("Transaction does not found with ID:{Id}", request.UpdateTransactionDto.TransactionGroupId);
        return Result.Failure<GetTransactionDto>(ApplicationError.TransactionGroupNotExists(request.UpdateTransactionDto.TransactionGroupId.ToString()!));
      }
    }

    var transactionWithSameName = await _transactionRepository.GetQueryAsync(TransactionQueryCriteria.FindDuplicatedNameExludingId(request.UpdateTransactionDto), cancellationToken: cancellationToken);

    if (transactionWithSameName.Count > 0)
    {
      _logger.LogError("Transaction already exists with Name:{Name}", request.UpdateTransactionDto.Name);
      return Result.Failure<GetTransactionDto>(ApplicationError.NameAlreadyExistsError(request.UpdateTransactionDto.Name));
    }

    var transaction = await _transactionRepository.GetByIdAsync(request.UpdateTransactionDto.Id, cancellationToken);

    if (transaction is null)
    {
      _logger.LogError("Transaction not found with ID:{Id}", request.UpdateTransactionDto.Id);
      return Result.Failure<GetTransactionDto>(ApplicationError.EntityNotFoundError(request.UpdateTransactionDto.Id.ToString()));
    }

    transaction.Update(
      request.UpdateTransactionDto.Name,
      request.UpdateTransactionDto.Description,
      request.UpdateTransactionDto.Value,
      request.UpdateTransactionDto.TransactionType,
      request.UpdateTransactionDto.TransactionDate,
      transactionGroup
    );

    await _transactionRepository.UpdateAsync(transaction, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Transaction updated with ID:{Id}", request.UpdateTransactionDto.Id);

    return Result.Success(_mapper.Map<GetTransactionDto>(transaction));
  }
}
