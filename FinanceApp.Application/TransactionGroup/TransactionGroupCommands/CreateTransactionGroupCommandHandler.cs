using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupCommands;

public class CreateTransactionGroupCommandHandler : ICommandHandler<CreateTransactionGroupCommand, Result<GetTransactionGroupDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.TransactionGroup> _transactionGroupRepository;
  private readonly ILogger<CreateTransactionGroupCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CreateTransactionGroupCommandHandler(IMapper mapper,
                                         IUnitOfWork unitOfWork,
                                         IRepository<Domain.Entities.TransactionGroup> transactionGroupRepository,
                                         ILogger<CreateTransactionGroupCommandHandler> logger,
                                         IUserRepository userRepository,
                                         IHttpContextAccessor httpContextAccessor)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _transactionGroupRepository = transactionGroupRepository;
    _logger = logger;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;
  }

  /// <inheritdoc />
  public async Task<Result<GetTransactionGroupDto>> Handle(CreateTransactionGroupCommand request, CancellationToken cancellationToken)
  {
    var transactionGroup = await _transactionGroupRepository.GetQueryAsync(TransactionQueryCriteria.FindDuplicatedName(request.CreateTransactionGroupDto), cancellationToken: cancellationToken);

    if (transactionGroup.Count > 0)
    {
      _logger.LogError("Transaction Group already exists with name:{Name}", request.CreateTransactionGroupDto.Name);
      return Result.Failure<GetTransactionGroupDto>(ApplicationError.NameAlreadyExistsError(request.CreateTransactionGroupDto.Name));
    }

    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (userEmail is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<GetTransactionGroupDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetUserByEmailAsync(userEmail!);

    var result = await _transactionGroupRepository.CreateAsync(new Domain.Entities.TransactionGroup(
                                                                            request.CreateTransactionGroupDto.Name,
                                                                            request.CreateTransactionGroupDto.Description,
                                                                            request.CreateTransactionGroupDto.GroupIcon,
                                                                            user!), cancellationToken);


    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Transaction Group created with ID:{Id}", result.Id);
    var asd = _mapper.Map<GetTransactionGroupDto>(result);
    return Result.Success(_mapper.Map<GetTransactionGroupDto>(result));
  }
}
