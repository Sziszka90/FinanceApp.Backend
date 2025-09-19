using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Converters;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.CreateTransaction;

public class CreateTransactionCommandHandler : ICommandHandler<CreateTransactionCommand, Result<GetTransactionDto>>
{
  private readonly ILogger<CreateTransactionCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserService _userService;
  private readonly IExchangeRateRepository _exchangeRateRepository;

  public CreateTransactionCommandHandler(
    ILogger<CreateTransactionCommandHandler> logger,
    IMapper mapper,
    ITransactionRepository transactionRepository,
    ITransactionGroupRepository transactionGroupRepository,
    IUnitOfWork unitOfWork,
    IUserService userService,
    IExchangeRateRepository exchangeRateRepository)
  {
    _logger = logger;
    _mapper = mapper;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
    _userService = userService;
    _exchangeRateRepository = exchangeRateRepository;
  }

  /// <inheritdoc />
  public async Task<Result<GetTransactionDto>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<GetTransactionDto>(user.ApplicationError!);
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

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(noTracking: true, cancellationToken: cancellationToken);

    var convertedAmount = CurrencyConverter.ConvertToUserCurrency(
      request.CreateTransactionDto.Value.Amount,
      request.CreateTransactionDto.Value.Currency,
      CurrencyEnum.EUR,
      exchangeRates);

    var transaction = await _transactionRepository.CreateAsync(new Transaction(
                                                                    request.CreateTransactionDto.Name,
                                                                    request.CreateTransactionDto.Description,
                                                                    request.CreateTransactionDto.TransactionType,
                                                                    new Money()
                                                                    {
                                                                      Amount = convertedAmount,
                                                                      Currency = CurrencyEnum.EUR
                                                                    },
                                                                    transactionGroup,
                                                                    request.CreateTransactionDto.TransactionDate,
                                                                    user.Data!), cancellationToken);


    await _unitOfWork.SaveChangesAsync(cancellationToken);
    _logger.LogInformation("Transaction created with ID:{Id}", transaction.Id);
    return Result.Success(_mapper.Map<GetTransactionDto>(transaction));
  }
}
