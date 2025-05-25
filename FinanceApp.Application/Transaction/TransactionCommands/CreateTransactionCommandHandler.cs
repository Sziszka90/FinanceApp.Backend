using System.Security.Claims;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public class CreateTransactionCommandHandler : ICommandHandler<CreateTransactionCommand, Result<GetTransactionDto>>
{
  private readonly ILogger<CreateTransactionCommandHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.Transaction> _transactionRepository;
  private readonly IUserRepository _userRepository;
  private readonly IRepository<Domain.Entities.TransactionGroup> _transactionGroupRepository;

  public CreateTransactionCommandHandler(ILogger<CreateTransactionCommandHandler> logger,
                                     IHttpContextAccessor httpContextAccessor,
                                     IMapper mapper,
                                     IUnitOfWork unitOfWork,
                                     IRepository<Domain.Entities.Transaction> transactionRepository,
                                     IUserRepository userRepository,
                                     IRepository<Domain.Entities.TransactionGroup> transactionGroupRepository)
  {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _transactionRepository = transactionRepository;
    _userRepository = userRepository;
    _transactionGroupRepository = transactionGroupRepository;
  }

  /// <inheritdoc />
  public async Task<Result<GetTransactionDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
  {
    Domain.Entities.TransactionGroup? transactionGroup = null;

    if (request.CreateTransactionDto.TransactionGroupId is not null)
    {
      transactionGroup = await _transactionGroupRepository.GetByIdAsync((Guid)request.CreateTransactionDto.TransactionGroupId, cancellationToken);

      if (transactionGroup is null)
      {
        _logger.LogError("Transaction Group not found with ID:{Id}", request.CreateTransactionDto.TransactionGroupId);
        return Result.Failure<GetTransactionDto>(ApplicationError.TransactionGroupNotExists(request.CreateTransactionDto.TransactionGroupId.ToString()!));
      }
    }

    var criteria = TransactionQueryCriteria.FindDuplicatedName(request.CreateTransactionDto);

    var transactionsWithSameName = await _transactionRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    if (transactionsWithSameName.Count > 0)
    {
      _logger.LogError("Transaction already exists with Name:{Name}", request.CreateTransactionDto.Name);
      return Result.Failure<GetTransactionDto>(ApplicationError.NameAlreadyExistsError(request.CreateTransactionDto.Name));
    }

    var httpContext = _httpContextAccessor.HttpContext;

    var currentUserName = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (currentUserName is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<GetTransactionDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetByUserNameAsync(currentUserName!);

    var transaction = await _transactionRepository.CreateAsync(new Domain.Entities.Transaction(
                                                                    request.CreateTransactionDto.Name,
                                                                    request.CreateTransactionDto.Description,
                                                                    request.CreateTransactionDto.TransactionType,
                                                                    request.CreateTransactionDto.Value,
                                                                    transactionGroup,
                                                                    request.CreateTransactionDto.TransactionDate,
                                                                    user!), cancellationToken);


    await _unitOfWork.SaveChangesAsync(cancellationToken);
    _logger.LogInformation("Transaction created with ID:{Id}", transaction.Id);
    return Result.Success(_mapper.Map<GetTransactionDto>(transaction));
  }
}
