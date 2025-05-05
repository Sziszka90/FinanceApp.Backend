using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;

public class UpdateExpenseGroupCommandHandler : ICommandHandler<UpdateExpenseGroupCommand, Result<GetExpenseTransactionGroupDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.ExpenseTransactionGroup> _expenseTransactionGroupRepository;
  private readonly ILogger<UpdateExpenseGroupCommandHandler> _logger;

  #endregion

  #region Constructors

  public UpdateExpenseGroupCommandHandler(IMapper mapper,
                                          IUnitOfWork unitOfWork,
                                          IRepository<Domain.Entities.ExpenseTransactionGroup> expenseTransactionGroupRepository,
                                          ILogger<UpdateExpenseGroupCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _expenseTransactionGroupRepository = expenseTransactionGroupRepository;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<GetExpenseTransactionGroupDto>> Handle(UpdateExpenseGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _expenseTransactionGroupRepository.GetByIdAsync(request.UpdateExpenseTransactionGroupDto.Id, cancellationToken);

    if (transactionGroup is null)
    {
      _logger.LogError("Expense Transaction Group does not found with ID:{Id}", request.UpdateExpenseTransactionGroupDto.Id);
      return Result.Failure<GetExpenseTransactionGroupDto>(ApplicationError.TransactionGroupNotExists(request.UpdateExpenseTransactionGroupDto.Id.ToString()));
    }

    var transactionGroupWithSameName = await _expenseTransactionGroupRepository.GetQueryAsync(ExpenseQueryCriteria.FindDuplicatedNameExludingId(request.UpdateExpenseTransactionGroupDto), cancellationToken: cancellationToken);

    if (transactionGroupWithSameName.Count > 0)
    {
      _logger.LogError("Expense Transaction Group already exists with Name:{Name}", request.UpdateExpenseTransactionGroupDto.Name);

      return Result.Failure<GetExpenseTransactionGroupDto>(ApplicationError.NameAlreadyExistsError(request.UpdateExpenseTransactionGroupDto.Name));
    }

    transactionGroup!.Update(request.UpdateExpenseTransactionGroupDto.Name,
                             request.UpdateExpenseTransactionGroupDto.Description,
                             request.UpdateExpenseTransactionGroupDto.Icon,
                             request.UpdateExpenseTransactionGroupDto.Limit);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Expense Transaction Group updated with ID:{Id}", request.UpdateExpenseTransactionGroupDto.Id);


    return Result.Success(_mapper.Map<GetExpenseTransactionGroupDto>(await _expenseTransactionGroupRepository.UpdateAsync(transactionGroup, cancellationToken)));
  }

  #endregion
}