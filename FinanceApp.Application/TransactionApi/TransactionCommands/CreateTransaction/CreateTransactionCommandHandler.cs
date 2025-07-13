using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.CreateTransaction;

public class CreateTransactionCommandHandler : ICommandHandler<CreateTransactionCommand, Result<GetTransactionDto>>
{
  private readonly ILogger<CreateTransactionCommandHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IMapper _mapper;
  private readonly IRepository<Transaction> _transactionRepository;
  private readonly IUserRepository _userRepository;
  private readonly IRepository<TransactionGroup> _transactionGroupRepository;
  private readonly IUnitOfWork _unitOfWork;

  public CreateTransactionCommandHandler(
    ILogger<CreateTransactionCommandHandler> logger,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper,
    IRepository<Transaction> transactionRepository,
    IUserRepository userRepository,
    IRepository<TransactionGroup> transactionGroupRepository,
    IUnitOfWork unitOfWork)
  {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
    _mapper = mapper;
    _transactionRepository = transactionRepository;
    _userRepository = userRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
  }

  /// <inheritdoc />
  public async Task<Result<GetTransactionDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    var user = await _userRepository.GetUserByEmailAsync(userEmail!, noTracking: false, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with email:{Email}", userEmail);
      return Result.Failure<GetTransactionDto>(ApplicationError.UserNotFoundError(userEmail!));
    }

    TransactionGroup? transactionGroup = null;
    if (request.CreateTransactionDto.TransactionGroupId is not null)
    {
      transactionGroup = await _transactionGroupRepository.GetByIdAsync(new Guid(request.CreateTransactionDto.TransactionGroupId), noTracking: false, cancellationToken: cancellationToken);

      if (transactionGroup is null)
      {
        _logger.LogError("Transaction Group not found with ID:{Id}", request.CreateTransactionDto.TransactionGroupId);
        return Result.Failure<GetTransactionDto>(ApplicationError.TransactionGroupNotExists(request.CreateTransactionDto.TransactionGroupId!));
      }
    }

    var transaction = await _transactionRepository.CreateAsync(new Transaction(
                                                                    request.CreateTransactionDto.Name,
                                                                    request.CreateTransactionDto.Description,
                                                                    request.CreateTransactionDto.TransactionType,
                                                                    request.CreateTransactionDto.Value,
                                                                    transactionGroup,
                                                                    request.CreateTransactionDto.TransactionDate,
                                                                    user!), cancellationToken);


    await _unitOfWork.SaveChangesAsync(cancellationToken);
    _logger.LogDebug("Transaction created with ID:{Id}", transaction.Id);
    return Result.Success(_mapper.Map<GetTransactionDto>(transaction));
  }
}
