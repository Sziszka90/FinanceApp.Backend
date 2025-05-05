using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;

public class CreateExpenseGroupCommandHandler : ICommandHandler<CreateExpenseGroupCommand, Result<GetExpenseTransactionGroupDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.ExpenseTransactionGroup> _expenseTransactionGroupRepository;
  private readonly ILogger<CreateExpenseGroupCommandHandler> _logger;

  #endregion

  #region Constructors

  public CreateExpenseGroupCommandHandler(IMapper mapper,
                                          IUnitOfWork unitOfWork,
                                          IRepository<Domain.Entities.ExpenseTransactionGroup> expenseTransactionGroupRepository,
                                          ILogger<CreateExpenseGroupCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _expenseTransactionGroupRepository = expenseTransactionGroupRepository;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<GetExpenseTransactionGroupDto>> Handle(CreateExpenseGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _expenseTransactionGroupRepository.GetQueryAsync(ExpenseQueryCriteria.FindDuplicatedName(request.CreateExpenseTransactionGroupDto), cancellationToken: cancellationToken);

    if (transactionGroup.Count > 0)
    {
      _logger.LogError("Expense Transaction Group already exists with Name:{Name}", request.CreateExpenseTransactionGroupDto.Name);
      return Result.Failure<GetExpenseTransactionGroupDto>(ApplicationError.NameAlreadyExistsError(request.CreateExpenseTransactionGroupDto.Name));
    }

    var expenseGroup = await _expenseTransactionGroupRepository.CreateAsync(new Domain.Entities.ExpenseTransactionGroup(
                                                                              request.CreateExpenseTransactionGroupDto.Name,
                                                                              request.CreateExpenseTransactionGroupDto.Description,
                                                                              request.CreateExpenseTransactionGroupDto.Icon,
                                                                              request.CreateExpenseTransactionGroupDto.Limit), cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Expense Transaction Group created with id: {Id}", expenseGroup.Id);

    return Result.Success(_mapper.Map<GetExpenseTransactionGroupDto>(expenseGroup));
  }

  #endregion
}