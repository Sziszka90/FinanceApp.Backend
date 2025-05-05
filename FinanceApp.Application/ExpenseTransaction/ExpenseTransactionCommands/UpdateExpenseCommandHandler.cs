using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;

public class UpdateExpenseCommandHandler : ICommandHandler<UpdateExpenseCommand, Result<GetExpenseTransactionDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.ExpenseTransaction> _expenseTransactionRepository;
  private readonly IRepository<Domain.Entities.ExpenseTransactionGroup> _expenseTransactionGroupRepository;
  private readonly ILogger<UpdateExpenseCommandHandler> _logger;

  #endregion

  #region Constructors

  public UpdateExpenseCommandHandler(IMapper mapper,
                                     IUnitOfWork unitOfWork,
                                     IRepository<Domain.Entities.ExpenseTransaction> expenseTransactionRepository,
                                     IRepository<Domain.Entities.ExpenseTransactionGroup> expenseTransactionGroupRepository,
                                     ILogger<UpdateExpenseCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _expenseTransactionRepository = expenseTransactionRepository;
    _expenseTransactionGroupRepository = expenseTransactionGroupRepository;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<GetExpenseTransactionDto>> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
  {
    Domain.Entities.ExpenseTransactionGroup? transactionGroup = null;

    if (request.UpdateExpenseTransactionDto.TransactionGroupId is not null)
    {
      transactionGroup = await _expenseTransactionGroupRepository.GetByIdAsync((Guid)request.UpdateExpenseTransactionDto.TransactionGroupId, cancellationToken);

      if (transactionGroup is null)
      {
        _logger.LogError("Expense Transaction does not found with ID:{Id}", request.UpdateExpenseTransactionDto.TransactionGroupId);
        return Result.Failure<GetExpenseTransactionDto>(ApplicationError.TransactionGroupNotExists(request.UpdateExpenseTransactionDto.TransactionGroupId.ToString()!));
      }
    }

    var transactionWithSameName = await _expenseTransactionRepository.GetQueryAsync(ExpenseQueryCriteria.FindDuplicatedNameExludingId(request.UpdateExpenseTransactionDto), cancellationToken: cancellationToken);

    if (transactionWithSameName.Count > 0)
    {
      _logger.LogError("Expense Transaction already exists with Name:{Name}", request.UpdateExpenseTransactionDto.Name);
      return Result.Failure<GetExpenseTransactionDto>(ApplicationError.NameAlreadyExistsError(request.UpdateExpenseTransactionDto.Name));
    }

    var expense = await _expenseTransactionRepository.GetByIdAsync(request.UpdateExpenseTransactionDto.Id, cancellationToken);

    if (expense is null)
    {
      _logger.LogError("Expense Transaction not found with ID:{Id}", request.UpdateExpenseTransactionDto.Id);
      return Result.Failure<GetExpenseTransactionDto>(ApplicationError.EntityNotFoundError(request.UpdateExpenseTransactionDto.Id.ToString()));
    }

    expense.Update(
      request.UpdateExpenseTransactionDto.Name,
      request.UpdateExpenseTransactionDto.Description,
      request.UpdateExpenseTransactionDto.Value,
      request.UpdateExpenseTransactionDto.DueDate,
      transactionGroup,
      request.UpdateExpenseTransactionDto.Priority
    );

    await _expenseTransactionRepository.UpdateAsync(expense, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Expense Transaction updated with ID:{Id}", request.UpdateExpenseTransactionDto.Id);

    return Result.Success(_mapper.Map<GetExpenseTransactionDto>(expense));
  }

  #endregion
}