using System.Text.Json;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Hubs;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.UploadCsv;

public class LLMProcessorCommandHandler : ICommandHandler<LLMProcessorCommand, Result<bool>>
{
  private readonly ILogger<LLMProcessorCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ISignalRService _signalRService;

  public LLMProcessorCommandHandler(
    ILogger<LLMProcessorCommandHandler> logger,
    IUserRepository userRepository,
    ITransactionRepository transactionRepository,
    ITransactionGroupRepository transactionGroupRepository,
    IExchangeRateRepository exchangeRateRepository,
    IUnitOfWork unitOfWork,
    ISignalRService signalRService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _exchangeRateRepository = exchangeRateRepository;
    _unitOfWork = unitOfWork;
    _signalRService = signalRService;
  }

  /// <inheritdoc />
  public async Task<Result<bool>> Handle(LLMProcessorCommand request, CancellationToken cancellationToken)
  {
    var matchedTransactions = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(request.ResponseDto.Response);

    var user = await _userRepository.GetByIdAsync(new Guid(request.ResponseDto.UserId), noTracking: false, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with ID: {UserId}", request.ResponseDto.UserId);
      return Result.Failure<bool>(ApplicationError.UserNotFoundError());
    }

    var existingTransactionGroups = await _transactionGroupRepository.GetAllByUserIdAsync(user.Id, noTracking: false, cancellationToken: cancellationToken);
    var existingTransactions = await _transactionRepository.GetAllByUserIdAsync(user.Id, noTracking: false, cancellationToken: cancellationToken);

    if (existingTransactions.Count == 0)
    {
      _logger.LogError("No valid transactions found.");
      return Result.Failure<bool>(ApplicationError.DefaultError("Transaction list is empty."));
    }

    if(existingTransactionGroups.Count == 0)
    {
      _logger.LogError("No valid transaction groups found.");
      return Result.Failure<bool>(ApplicationError.DefaultError("Transaction group list is empty."));
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

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    await _signalRService.SendToClientGroupMethodAsync(user.Email.ToString(), HubConstants.TRANSACTIONS_MATCHED_NOTIFICATION, HubConstants.REFRESH_TRANSACTIONS);

    var allTransactions = await _transactionRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);

    _logger.LogDebug("Processed matched transactions for user: {UserId}", user.Id);
    return Result.Success(true);
  }

  private decimal ConvertToUserCurrency(decimal amount, CurrencyEnum fromCurrency, CurrencyEnum toCurrency, List<Domain.Entities.ExchangeRate> rates)
  {
    if (fromCurrency == toCurrency)
      return Math.Round(amount, 2);
    var rate = rates.FirstOrDefault(r => r.BaseCurrency == fromCurrency.ToString() && r.TargetCurrency == toCurrency.ToString());

    return Math.Round(amount * rate!.Rate, 2);
  }
}
