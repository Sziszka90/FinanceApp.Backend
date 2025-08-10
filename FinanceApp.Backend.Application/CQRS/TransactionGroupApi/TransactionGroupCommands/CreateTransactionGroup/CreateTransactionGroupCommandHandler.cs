using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;

public class CreateTransactionGroupCommandHandler : ICommandHandler<CreateTransactionGroupCommand, Result<GetTransactionGroupDto>>
{
  private readonly ILogger<CreateTransactionGroupCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserService _userService;

  public CreateTransactionGroupCommandHandler(
    ILogger<CreateTransactionGroupCommandHandler> logger,
    IMapper mapper,
    ITransactionGroupRepository transactionGroupRepository,
    IUnitOfWork unitOfWork,
    IUserService userService)
  {
    _logger = logger;
    _mapper = mapper;
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
    _userService = userService;
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

    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<GetTransactionGroupDto>(user.ApplicationError!);
    }

    var result = await _transactionGroupRepository.CreateAsync(new TransactionGroup(
                                                                request.CreateTransactionGroupDto.Name,
                                                                request.CreateTransactionGroupDto.Description,
                                                                request.CreateTransactionGroupDto.GroupIcon,
                                                                user.Data!));


    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Transaction Group created with ID:{Id}", result.Id);

    return Result.Success(_mapper.Map<GetTransactionGroupDto>(result));
  }
}
