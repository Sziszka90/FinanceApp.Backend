using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;

public class CreateExpenseCommandHandler : ICommandHandler<CreateExpenseCommand, Result<GetExpenseTransactionDto>>
{
  #region Members

  private readonly ILogger<CreateExpenseCommandHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.ExpenseTransaction> _expenseTransactionRepository;
  private readonly IUserRepository _userRepository;
  private readonly IRepository<Domain.Entities.ExpenseTransactionGroup> _expenseTransactionGroupRepository;

  #endregion

  #region Constructors

  public CreateExpenseCommandHandler(ILogger<CreateExpenseCommandHandler> logger,
                                     IHttpContextAccessor httpContextAccessor,
                                     IMapper mapper,
                                     IUnitOfWork unitOfWork,
                                     IRepository<Domain.Entities.ExpenseTransaction> expenseTransactionRepository,
                                     IUserRepository userRepository,
                                     IRepository<Domain.Entities.ExpenseTransactionGroup> expenseTransactionGroupRepository)
  {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _expenseTransactionRepository = expenseTransactionRepository;
    _userRepository = userRepository;
    _expenseTransactionGroupRepository = expenseTransactionGroupRepository;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<GetExpenseTransactionDto>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
  {
    Domain.Entities.ExpenseTransactionGroup? transactionGroup = null;

    if (request.CreateExpenseTransactionDto.TransactionGroupId is not null)
    {
      transactionGroup = await _expenseTransactionGroupRepository.GetByIdAsync((Guid)request.CreateExpenseTransactionDto.TransactionGroupId, cancellationToken);

      if (transactionGroup is null)
      {
        _logger.LogError("Expense Transaction Group not found with ID:{Id}", request.CreateExpenseTransactionDto.TransactionGroupId);
        return Result.Failure<GetExpenseTransactionDto>(ApplicationError.TransactionGroupNotExists(request.CreateExpenseTransactionDto.TransactionGroupId.ToString()!));
      }
    }

    var criteria = ExpenseQueryCriteria.FindDuplicatedName(request.CreateExpenseTransactionDto);

    var transactionsWithSameName = await _expenseTransactionRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    if (transactionsWithSameName.Count > 0)
    {
      _logger.LogError("Expense Transaction already exists with Name:{Name}", request.CreateExpenseTransactionDto.Name);
      return Result.Failure<GetExpenseTransactionDto>(ApplicationError.NameAlreadyExistsError(request.CreateExpenseTransactionDto.Name));
    }

    var httpContext = _httpContextAccessor.HttpContext;

    var currentUserName = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (currentUserName is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<GetExpenseTransactionDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetByUserNameAsync(currentUserName!);

    var expense = await _expenseTransactionRepository.CreateAsync(new Domain.Entities.ExpenseTransaction(
                                                                    request.CreateExpenseTransactionDto.Name,
                                                                    request.CreateExpenseTransactionDto.Description,
                                                                    request.CreateExpenseTransactionDto.Value,
                                                                    request.CreateExpenseTransactionDto.DueDate,
                                                                    transactionGroup,
                                                                    request.CreateExpenseTransactionDto.Priority,
                                                                    user!), cancellationToken);


    await _unitOfWork.SaveChangesAsync(cancellationToken);
    _logger.LogInformation("Expense Transaction created with ID:{Id}", expense.Id);
    return Result.Success(_mapper.Map<GetExpenseTransactionDto>(expense));
  }

  #endregion
}
