using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UpdateTransaction;

public class UpdateTransactionCommandHandler : ICommandHandler<UpdateTransactionCommand, Result<GetTransactionDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Transaction> _transactionRepository;
  private readonly IRepository<TransactionGroup> _transactionGroupRepository;
  private readonly ILogger<UpdateTransactionCommandHandler> _logger;
  private readonly IExchangeRateService _exchangeRateService;

  public UpdateTransactionCommandHandler(
    IMapper mapper,
    IUnitOfWork unitOfWork,
    IRepository<Transaction> transactionRepository,
    IRepository<TransactionGroup> transactionGroupRepository,
    ILogger<UpdateTransactionCommandHandler> logger,
    IExchangeRateService exchangeRateService)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _logger = logger;
    _exchangeRateService = exchangeRateService;
  }

  /// <inheritdoc />
  public async Task<Result<GetTransactionDto>> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
  {
    TransactionGroup? transactionGroup = null;

    if (request.UpdateTransactionDto.TransactionGroupId is not null)
    {
      transactionGroup = await _transactionGroupRepository.GetByIdAsync((Guid)request.UpdateTransactionDto.TransactionGroupId, noTracking: false, cancellationToken: cancellationToken);

      if (transactionGroup is null)
      {
        _logger.LogError("Transaction does not found with ID:{Id}", request.UpdateTransactionDto.TransactionGroupId);
        return Result.Failure<GetTransactionDto>(ApplicationError.TransactionGroupNotExists(request.UpdateTransactionDto.TransactionGroupId.ToString()!));
      }
    }

    var transaction = await _transactionRepository.GetByIdAsync(request.Id, noTracking: false, cancellationToken: cancellationToken);

    if (transaction is null)
    {
      _logger.LogError("Transaction not found with ID:{Id}", request.Id);
      return Result.Failure<GetTransactionDto>(ApplicationError.EntityNotFoundError(request.Id.ToString()));
    }

    decimal valueInBaseCurrency = transaction.ValueInBaseCurrency;

    bool currencyChanged = transaction.Value.Currency != request.UpdateTransactionDto.Value.Currency;
    bool amountChanged = transaction.Value.Amount != request.UpdateTransactionDto.Value.Amount;

    if ((currencyChanged || amountChanged) && request.UpdateTransactionDto.Value.Currency != CurrencyEnum.EUR)
    {
      var valueInBaseCurrencyResult = await _exchangeRateService.ConvertAmountAsync(
        request.UpdateTransactionDto.Value.Amount,
        request.UpdateTransactionDto.TransactionDate,
        request.UpdateTransactionDto.Value.Currency.ToString(),
        CurrencyEnum.EUR.ToString(),
        cancellationToken);

      if (!valueInBaseCurrencyResult.IsSuccess)
      {
        _logger.LogError("Failed to convert amount: {Error}", valueInBaseCurrencyResult.ApplicationError?.Message);
        return Result.Failure<GetTransactionDto>(valueInBaseCurrencyResult.ApplicationError!);
      }
      valueInBaseCurrency = valueInBaseCurrencyResult.Data;
    }

    transaction.Update(
      request.UpdateTransactionDto.Name,
      request.UpdateTransactionDto.Description,
      new Money()
      {
        Amount = request.UpdateTransactionDto.Value.Amount,
        Currency = request.UpdateTransactionDto.Value.Currency
      },
      valueInBaseCurrency,
      request.UpdateTransactionDto.TransactionType,
      request.UpdateTransactionDto.TransactionDate,
      transactionGroup
    );

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Transaction updated with ID:{Id}", request.Id);

    return Result.Success(_mapper.Map<GetTransactionDto>(transaction));
  }
}
