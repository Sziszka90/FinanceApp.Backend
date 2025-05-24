using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupCommands;

public class CreateIncomeGroupCommandHandler : ICommandHandler<CreateIncomeGroupCommand, Result<GetIncomeTransactionGroupDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.IncomeTransactionGroup> _incomeTransactionGroupRepository;
  private readonly ILogger<CreateIncomeGroupCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;

  #endregion

  #region Constructors

  public CreateIncomeGroupCommandHandler(IMapper mapper,
                                         IUnitOfWork unitOfWork,
                                         IRepository<Domain.Entities.IncomeTransactionGroup> incomeTransactionGroupRepository,
                                         ILogger<CreateIncomeGroupCommandHandler> logger,
                                         IUserRepository userRepository,
                                         IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _incomeTransactionGroupRepository = incomeTransactionGroupRepository;
    _logger = logger;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<GetIncomeTransactionGroupDto>> Handle(CreateIncomeGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _incomeTransactionGroupRepository.GetQueryAsync(IncomeQueryCriteria.FindDuplicatedName(request.CreateIncomeTransactionGroupDto), cancellationToken: cancellationToken);

    if (transactionGroup.Count > 0)
    {
      _logger.LogError("Income Transaction Group already exists with name:{Name}", request.CreateIncomeTransactionGroupDto.Name);
      return Result.Failure<GetIncomeTransactionGroupDto>(ApplicationError.NameAlreadyExistsError(request.CreateIncomeTransactionGroupDto.Name));
    }

    var httpContext = _httpContextAccessor.HttpContext;

    var currentUserName = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (currentUserName is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<GetIncomeTransactionGroupDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetByUserNameAsync(currentUserName!);

    var incomeGroup = await _incomeTransactionGroupRepository.CreateAsync(new Domain.Entities.IncomeTransactionGroup(
                                                                            request.CreateIncomeTransactionGroupDto.Name,
                                                                            request.CreateIncomeTransactionGroupDto.Description,
                                                                            request.CreateIncomeTransactionGroupDto.Icon,
                                                                            user!), cancellationToken);


    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Income Transaction Group created with ID:{Id}", incomeGroup.Id);
    return Result.Success(_mapper.Map<GetIncomeTransactionGroupDto>(incomeGroup));
  }

  #endregion
}
