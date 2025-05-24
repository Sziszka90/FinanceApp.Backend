using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class CreateIncomeCommandHandler : ICommandHandler<CreateIncomeCommand, Result<GetIncomeTransactionDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.IncomeTransaction> _incomeTransactionRepository;
  private readonly IRepository<Domain.Entities.IncomeTransactionGroup> _incomeTransactionGroupRepository;
  private readonly ILogger<CreateIncomeCommandHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IUserRepository _userRepository;

  #endregion

  #region Constructors

  public CreateIncomeCommandHandler(IMapper mapper,
                                    IUnitOfWork unitOfWork,
                                    IRepository<Domain.Entities.IncomeTransaction> incomeTransactionRepository,
                                    IRepository<Domain.Entities.IncomeTransactionGroup> incomeTransactionGroupRepository,
                                    ILogger<CreateIncomeCommandHandler> logger,
                                    IHttpContextAccessor httpContextAccessor,
                                    IUserRepository userRepository)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _incomeTransactionRepository = incomeTransactionRepository;
    _incomeTransactionGroupRepository = incomeTransactionGroupRepository;
    _logger = logger;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<GetIncomeTransactionDto>> Handle(CreateIncomeCommand request, CancellationToken cancellationToken)
  {
    Domain.Entities.IncomeTransactionGroup? transactionGroup = null;

    if (request.CreateIncomeTransactionDto.TransactionGroupId is not null)
    {
      transactionGroup = await _incomeTransactionGroupRepository.GetByIdAsync((Guid)request.CreateIncomeTransactionDto.TransactionGroupId, cancellationToken);

      if (transactionGroup is null)
      {
        _logger.LogError("Income Transaction Group not found with ID:{Id}", request.CreateIncomeTransactionDto.TransactionGroupId);
        return Result.Failure<GetIncomeTransactionDto>(ApplicationError.TransactionGroupNotExists(request.CreateIncomeTransactionDto.TransactionGroupId.ToString()!));
      }
    }

    var criteria = IncomeQueryCriteria.FindDuplicatedName(request.CreateIncomeTransactionDto);

    var transactionsWithSameName = await _incomeTransactionRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    if (transactionsWithSameName.Count > 0)
    {
      _logger.LogError("Income Transaction already exists with Name:{Name}", request.CreateIncomeTransactionDto.Name);

      return Result.Failure<GetIncomeTransactionDto>(ApplicationError.NameAlreadyExistsError(request.CreateIncomeTransactionDto.Name));
    }

    var httpContext = _httpContextAccessor.HttpContext;

    var currentUserName = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (currentUserName is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<GetIncomeTransactionDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetByUserNameAsync(currentUserName!);

    var income = await _incomeTransactionRepository.CreateAsync(new Domain.Entities.IncomeTransaction(
                                                                  request.CreateIncomeTransactionDto.Name,
                                                                  request.CreateIncomeTransactionDto.Description,
                                                                  request.CreateIncomeTransactionDto.Value,
                                                                  request.CreateIncomeTransactionDto.DueDate,
                                                                  transactionGroup,
                                                                  user!), cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Income Transaction created with ID:{Id}", income.Id);


    return Result.Success(_mapper.Map<GetIncomeTransactionDto>(income));
  }

  #endregion
}
