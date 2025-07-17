using System.Text.Json;
using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.UploadCsv;

public class LLMProcessorCommandHandler : ICommandHandler<LLMProcessorCommand, Result<List<GetTransactionDto>>>
{
  private readonly ILogger<LLMProcessorCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IUserRepository _userRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IUnitOfWork _unitOfWork;

  public LLMProcessorCommandHandler(
    ILogger<LLMProcessorCommandHandler> logger,
    IMapper mapper,
    IUserRepository userRepository,
    ITransactionRepository transactionRepository,
    ITransactionGroupRepository transactionGroupRepository,
    IExchangeRateRepository exchangeRateRepository,
    IUnitOfWork unitOfWork)
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _exchangeRateRepository = exchangeRateRepository;
    _unitOfWork = unitOfWork;
  }

  /// <inheritdoc />
  public async Task<Result<List<GetTransactionDto>>> Handle(LLMProcessorCommand request, CancellationToken cancellationToken)
  {
    var matchedTransactions = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(request.ResponseDto.Response);

    var user = await _userRepository.GetByIdAsync(new Guid(request.ResponseDto.UserId), cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with ID: {UserId}", request.ResponseDto.UserId);
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.UserNotFoundError());
    }

    var existingTransactionGroups = await _transactionGroupRepository.GetAllAsync(false, cancellationToken: cancellationToken);
    var existingTransactions = await _transactionRepository.GetAllAsync(false, cancellationToken: cancellationToken);

    if (existingTransactions.Count == 0)
    {
      _logger.LogError("No valid transactions found.");
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.DefaultError("Transaction list is empty."));
    }

    if(existingTransactionGroups.Count == 0)
    {
      _logger.LogError("No valid transaction groups found.");
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.DefaultError("Transaction group list is empty."));
    }

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(noTracking: true, cancellationToken: cancellationToken);

    foreach (var transaction in existingTransactions)
    {
      var matchedGroup = matchedTransactions!.FirstOrDefault(dict => dict.ContainsKey(transaction.Name));
      var groupName = matchedGroup!.Values.FirstOrDefault();
      var group = existingTransactionGroups.FirstOrDefault(tg => tg.Name == groupName);

      transaction.TransactionGroup = group;

      if (transaction.Value.Currency != user!.BaseCurrency)
      {
        transaction.Value.Amount = ConvertToUserCurrency(transaction.Value.Amount, transaction.Value.Currency, user.BaseCurrency, exchangeRates);
        transaction.Value.Currency = user.BaseCurrency;
      }
    }

    var createdTransactions = await _transactionRepository.BatchCreateTransactionsAsync(existingTransactions, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    var allTransactions = await _transactionRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);

    _logger.LogDebug("Processed matched transactions for user: {UserId}", user.Id);
    return Result.Success(_mapper.Map<List<GetTransactionDto>>(allTransactions));
  }

  private decimal ConvertToUserCurrency(decimal amount, CurrencyEnum fromCurrency, CurrencyEnum toCurrency, List<FinanceApp.Domain.Entities.ExchangeRate> rates)
  {
    if (fromCurrency == toCurrency)
      return Math.Round(amount, 2);
    var rate = rates.FirstOrDefault(r => r.BaseCurrency == fromCurrency.ToString() && r.TargetCurrency == toCurrency.ToString());

    return Math.Round(amount * rate!.Rate, 2);
  }
}
