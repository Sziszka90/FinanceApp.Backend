using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class UpdateIncomeCommandHandler : ICommandHandler<UpdateIncomeCommand, Result<GetIncomeTransactionDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.IncomeTransaction> _incomeTransactionRepository;
  private readonly IRepository<Domain.Entities.IncomeTransactionGroup> _incomeTransactionGroupRepository;
  private readonly ILogger<UpdateIncomeCommandHandler> _logger;

  public UpdateIncomeCommandHandler(IMapper mapper,
                                    IUnitOfWork unitOfWork,
                                    IRepository<Domain.Entities.IncomeTransaction> incomeTransactionRepository,
                                    IRepository<Domain.Entities.IncomeTransactionGroup> incomeTransactionGroupRepository,
                                    ILogger<UpdateIncomeCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _incomeTransactionRepository = incomeTransactionRepository;
    _incomeTransactionGroupRepository = incomeTransactionGroupRepository;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result<GetIncomeTransactionDto>> Handle(UpdateIncomeCommand request, CancellationToken cancellationToken)
  {
    Domain.Entities.IncomeTransactionGroup? transactionGroup = null;

    if (request.UpdateIncomeTransactionDto.TransactionGroupId is not null)
    {
      transactionGroup = await _incomeTransactionGroupRepository.GetByIdAsync((Guid)request.UpdateIncomeTransactionDto.TransactionGroupId, cancellationToken);

      if (transactionGroup is null)
      {
        _logger.LogError("Income Transaction Group not found with ID:{Id}", request.UpdateIncomeTransactionDto.TransactionGroupId);
        return Result.Failure<GetIncomeTransactionDto>(ApplicationError.TransactionGroupNotExists(request.UpdateIncomeTransactionDto.TransactionGroupId.ToString()!));
      }
    }

    var transactionWithSameName = await _incomeTransactionRepository.GetQueryAsync(IncomeQueryCriteria.FindDuplicatedNameExludingId(request.UpdateIncomeTransactionDto), cancellationToken: cancellationToken);

    if (transactionWithSameName.Count > 0)
    {
      _logger.LogError("Income Transaction already exists with Name:{Name}", request.UpdateIncomeTransactionDto.Name);
      return Result.Failure<GetIncomeTransactionDto>(ApplicationError.NameAlreadyExistsError(request.UpdateIncomeTransactionDto.Name));
    }

    var income = await _incomeTransactionRepository.GetByIdAsync(request.UpdateIncomeTransactionDto.Id, cancellationToken);

    if (income is null)
    {
      _logger.LogError("Income Transaction not found with ID:{Id}", request.UpdateIncomeTransactionDto.Id);
      return Result.Failure<GetIncomeTransactionDto>(ApplicationError.EntityNotFoundError());
    }

    income.Update(
      request.UpdateIncomeTransactionDto.Name,
      request.UpdateIncomeTransactionDto.Description,
      request.UpdateIncomeTransactionDto.Value,
      request.UpdateIncomeTransactionDto.DueDate,
      transactionGroup
    );

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Income Transaction updated with ID:{Id}", request.UpdateIncomeTransactionDto.Id);
    return Result.Success(_mapper.Map<GetIncomeTransactionDto>(await _incomeTransactionRepository.UpdateAsync(income, cancellationToken)));
  }
}
