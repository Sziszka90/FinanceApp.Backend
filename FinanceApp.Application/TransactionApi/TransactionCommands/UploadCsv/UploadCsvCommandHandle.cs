using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using AutoMapper;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FinanceApp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.UploadCsv;

public class UploadCsvCommandHandler : ICommandHandler<UploadCsvCommand, Result<List<GetTransactionDto>>>
{
  private readonly ILogger<UploadCsvCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IUserRepository _userRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILLMClient _llmClient;

  public UploadCsvCommandHandler(
    ILogger<UploadCsvCommandHandler> logger,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository,
    ITransactionRepository transactionRepository,
    ITransactionGroupRepository transactionGroupRepository,
    IExchangeRateRepository exchangeRateRepository,
    IUnitOfWork unitOfWork,
    ILLMClient llmClient)
  {
    _logger = logger;
    _mapper = mapper;
    _httpContextAccessor = httpContextAccessor;
    _userRepository = userRepository;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _exchangeRateRepository = exchangeRateRepository;
    _unitOfWork = unitOfWork;
    _llmClient = llmClient;
  }

  /// <inheritdoc />
  public async Task<Result<List<GetTransactionDto>>> Handle(UploadCsvCommand request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (userEmail is null)
    {
      _logger.LogError("User is not logged in");
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetUserByEmailAsync(userEmail!);

    if (user is null)
    {
      _logger.LogError("User not found with email: {Email}", userEmail);
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.UserNotFoundError());
    }

    var transactions = await ImportTransactions(request.uploadCsvFileDto.File, user!);

    var existingTransactionGroups = await _transactionGroupRepository.GetAllAsync(cancellationToken: cancellationToken);
    var existingTransactionGroupNames = existingTransactionGroups.Select(tg => tg.Name).ToList();

    if (transactions.Count == 0)
    {
      _logger.LogError("No valid transactions found.");
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.ParsingError());
    }

    var matchedTransactionGroups = (await _llmClient.MatchTransactionGroup(transactions.Select(t => t.Name).ToList(), existingTransactionGroupNames, user!)).Data;

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(noTracking: true, cancellationToken: cancellationToken);

    foreach (var transaction in transactions)
    {
      var matchedGroup = matchedTransactionGroups!.FirstOrDefault(dict => dict.ContainsKey(transaction.Name));
      var groupName = matchedGroup!.Values.FirstOrDefault();
      var group = existingTransactionGroups.FirstOrDefault(tg => tg.Name == groupName);

      transaction.TransactionGroup = group;

      if (transaction.Value.Currency != user!.BaseCurrency)
      {
        transaction.Value.Amount = ConvertToUserCurrency(transaction.Value.Amount, transaction.Value.Currency, user.BaseCurrency, exchangeRates);
        transaction.Value.Currency = user.BaseCurrency;
      }
    }

    var createdTransactions = await _transactionRepository.BatchCreateTransactionsAsync(transactions, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    var allTransactions = await _transactionRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);

    _logger.LogDebug("CSV file uploaded and transactions created for user: {UserId}", user.Id);
    return Result.Success(_mapper.Map<List<GetTransactionDto>>(allTransactions));
  }

  private decimal ConvertToUserCurrency(decimal amount, CurrencyEnum fromCurrency, CurrencyEnum toCurrency, List<FinanceApp.Domain.Entities.ExchangeRate> rates)
  {
    if (fromCurrency == toCurrency)
      return Math.Round(amount, 2);
    var rate = rates.FirstOrDefault(r => r.BaseCurrency == fromCurrency.ToString() && r.TargetCurrency == toCurrency.ToString());

    return Math.Round(amount * rate!.Rate, 2);
  }

  private string CleanCsvField(string input)
  {
    if (string.IsNullOrEmpty(input)) return input;
    var cleaned = input.Trim();
    if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
      cleaned = cleaned.Substring(1, cleaned.Length - 2);
    cleaned = cleaned.Replace("\\", "");
    return cleaned;
  }

  private async Task<List<Transaction>> ImportTransactions(IFormFile file, User user)
  {
    var transactions = new List<Transaction>();

    using (var stream = file.OpenReadStream())
    using (var reader = new StreamReader(stream))
    {
      string? headerLine = await reader.ReadLineAsync();
      while (!reader.EndOfStream)
      {
        var line = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(line)) continue;
        var columns = line.Split(',');

        var amount = decimal.TryParse(CleanCsvField(columns[3]), NumberStyles.Number | NumberStyles.AllowThousands,
          new CultureInfo("hu-HU"), out var parsedAmount) ? parsedAmount : 0;

        var transaction = new Transaction(
          NormalizeSpaces(CleanCsvField(columns[5]) != "" ? CleanCsvField(columns[5]) : "Unknown"),
          CleanCsvField(columns[9]),
          amount < 0 ? TransactionTypeEnum.Expense : TransactionTypeEnum.Income,
          new Money
          {
            Amount = Math.Abs(amount),
            Currency = Enum.TryParse<CurrencyEnum>(CleanCsvField(columns[4]), out var currency) ? currency : CurrencyEnum.Unknown
          },
          null,
          DateTimeOffset.TryParse(CleanCsvField(columns[2]), out var date) ? date : DateTimeOffset.UtcNow,
          user!
        );
        transactions.Add(transaction);
      }
    }
    return transactions;
  }

  private string NormalizeSpaces(string input)
  {
    if (string.IsNullOrWhiteSpace(input)) return string.Empty;
    return Regex.Replace(input.Trim(), @"\s+", " ");
  }

}
