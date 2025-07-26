using System.Security.Claims;
using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;

public class CreateTransactionGroupCommandHandler : ICommandHandler<CreateTransactionGroupCommand, Result<GetTransactionGroupDto>>
{
  private readonly ILogger<CreateTransactionGroupCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.TransactionGroup> _transactionGroupRepository;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CreateTransactionGroupCommandHandler(
    ILogger<CreateTransactionGroupCommandHandler> logger,
    IMapper mapper,
    IRepository<TransactionGroup> transactionGroupRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor)
  {
    _logger = logger;
    _mapper = mapper;
    _transactionGroupRepository = transactionGroupRepository;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _httpContextAccessor = httpContextAccessor;
  }

  /// <inheritdoc />
  public async Task<Result<GetTransactionGroupDto>> Handle(CreateTransactionGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _transactionGroupRepository.GetQueryAsync(
      TransactionQueryCriteria.FindDuplicatedName(request.CreateTransactionGroupDto),
      noTracking: true,
      cancellationToken: cancellationToken);

    if (transactionGroup.Count > 0)
    {
      _logger.LogWarning("Transaction Group already exists with name:{Name}", request.CreateTransactionGroupDto.Name);
      return Result.Failure<GetTransactionGroupDto>(ApplicationError.NameAlreadyExistsError(request.CreateTransactionGroupDto.Name));
    }

    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    var user = await _userRepository.GetUserByEmailAsync(userEmail!, noTracking: false, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with email:{Email}", userEmail);
      return Result.Failure<GetTransactionGroupDto>(ApplicationError.UserNotFoundError(userEmail!));
    }

    var result = await _transactionGroupRepository.CreateAsync(new TransactionGroup(
                                                                request.CreateTransactionGroupDto.Name,
                                                                request.CreateTransactionGroupDto.Description,
                                                                request.CreateTransactionGroupDto.GroupIcon,
                                                                user!), cancellationToken);


    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Transaction Group created with ID:{Id}", result.Id);

    return Result.Success(_mapper.Map<GetTransactionGroupDto>(result));
  }
}
