using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;

public class CreateExpenseGroupCommandHandler : ICommandHandler<CreateExpenseGroupCommand, Result<GetExpenseTransactionGroupDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.ExpenseTransactionGroup> _expenseTransactionGroupRepository;
  private readonly ILogger<CreateExpenseGroupCommandHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IUserRepository _userRepository;

  public CreateExpenseGroupCommandHandler(IMapper mapper,
                                          IUnitOfWork unitOfWork,
                                          IRepository<Domain.Entities.ExpenseTransactionGroup> expenseTransactionGroupRepository,
                                          ILogger<CreateExpenseGroupCommandHandler> logger,
                                          IHttpContextAccessor httpContextAccessor,
                                          IUserRepository userRepository)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _expenseTransactionGroupRepository = expenseTransactionGroupRepository;
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
    _userRepository = userRepository;
  }

  /// <inheritdoc />
  public async Task<Result<GetExpenseTransactionGroupDto>> Handle(CreateExpenseGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _expenseTransactionGroupRepository.GetQueryAsync(ExpenseQueryCriteria.FindDuplicatedName(request.CreateExpenseTransactionGroupDto), cancellationToken: cancellationToken);

    if (transactionGroup.Count > 0)
    {
      _logger.LogError("Expense Transaction Group already exists with Name:{Name}", request.CreateExpenseTransactionGroupDto.Name);
      return Result.Failure<GetExpenseTransactionGroupDto>(ApplicationError.NameAlreadyExistsError(request.CreateExpenseTransactionGroupDto.Name));
    }

    var httpContext = _httpContextAccessor.HttpContext;

    var currentUserName = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (currentUserName is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<GetExpenseTransactionGroupDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetByUserNameAsync(currentUserName!);

    var expenseGroup = await _expenseTransactionGroupRepository.CreateAsync(new Domain.Entities.ExpenseTransactionGroup(
                                                                              request.CreateExpenseTransactionGroupDto.Name,
                                                                              request.CreateExpenseTransactionGroupDto.Description,
                                                                              request.CreateExpenseTransactionGroupDto.Icon,
                                                                              request.CreateExpenseTransactionGroupDto.Limit,
                                                                              user!), cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Expense Transaction Group created with id: {Id}", expenseGroup.Id);

    return Result.Success(_mapper.Map<GetExpenseTransactionGroupDto>(expenseGroup));
  }
}
