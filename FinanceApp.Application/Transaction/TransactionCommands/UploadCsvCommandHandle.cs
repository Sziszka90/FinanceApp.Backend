using System.Globalization;
using System.Security.Claims;
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

namespace FinanceApp.Application.Transaction.TransactionCommands;

public class UploadCsvCommandHandler : ICommandHandler<UploadCsvCommand, Result<List<GetTransactionDto>>>
{
  private readonly ILogger<UploadCsvCommandHandler> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IUserRepository _userRepository;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly IExchangeRateRepository _exchangeRateRepository;
  private readonly IMapper _mapper;

  private readonly ILLMClient _llmClient;

  public UploadCsvCommandHandler(ILogger<UploadCsvCommandHandler> logger,
                                     IHttpContextAccessor httpContextAccessor,
                                     IUserRepository userRepository,
                                     ITransactionRepository transactionRepository,
                                     IMapper mapper,
                                     ILLMClient llmClient,
                                     ITransactionGroupRepository transactionGroupRepository,
                                     IExchangeRateRepository exchangeRateRepository)
  {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;
    _userRepository = userRepository;
    _transactionRepository = transactionRepository;
    _mapper = mapper;
    _llmClient = llmClient;
    _transactionGroupRepository = transactionGroupRepository;
    _exchangeRateRepository = exchangeRateRepository;
  }

  // Helper to clean up quoted/escaped CSV fields
  private string CleanCsvField(string input)
  {
    if (string.IsNullOrEmpty(input)) return input;
    // Remove leading/trailing quotes and backslashes
    var cleaned = input.Trim();
    if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
      cleaned = cleaned.Substring(1, cleaned.Length - 2);
    cleaned = cleaned.Replace("\\", "");
    return cleaned;
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

    if (request.uploadCsvFileDto.File is null || request.uploadCsvFileDto.File.Length == 0)
    {
      _logger.LogError("File is null or empty");
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.FileEmptyError());
    }
    if (request.uploadCsvFileDto.File.ContentType != "text/csv")
    {
      _logger.LogError("Invalid file type: {ContentType}", request.uploadCsvFileDto.File.ContentType);
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.InvalidFileTypeError(request.uploadCsvFileDto.File.ContentType));
    }

    // Read and parse the CSV file
    var transactions = new List<Domain.Entities.Transaction>();

    using (var stream = request.uploadCsvFileDto.File.OpenReadStream())

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

        var transaction = new Domain.Entities.Transaction(
          CleanCsvField(columns[5]) != "" ? CleanCsvField(columns[5]) : "Unknown",
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

    var existingTransactionGroups = await _transactionGroupRepository.GetAllAsync(cancellationToken: cancellationToken);
    var existingTransactionGroupNames = existingTransactionGroups.Select(tg => tg.Name).ToList();

    var resultDictionaryList = (await _llmClient.CreateTransactionGroup(transactions.Select(t => t.Name).ToList(), existingTransactionGroupNames, user!, cancellationToken)).Data;

    foreach (var transaction in transactions)
    {
      var groupName = resultDictionaryList!.FirstOrDefault(dict => dict.ContainsKey(transaction.Name));
      var groupNameValue = groupName?.Values.FirstOrDefault();
      var group = existingTransactionGroups.FirstOrDefault(tg => tg.Name == groupNameValue);

      transaction.TransactionGroup = group;
    }

    var result = await _transactionRepository.CreateMultipleTransactionsAsync(transactions, cancellationToken);

    var exchangeRates = await _exchangeRateRepository.GetExchangeRatesAsync(cancellationToken);

    foreach (var transaction in result!)
    {
      if (transaction.Value.Currency != user!.BaseCurrency)
      {

        transaction.Value.Amount = ConvertToUserCurrency(transaction.Value.Amount, transaction.Value.Currency, user.BaseCurrency, exchangeRates);
        transaction.Value.Currency = user.BaseCurrency;
      }
    }
    var createdTransactions = _mapper.Map<List<GetTransactionDto>>(result);

    return Result.Success(createdTransactions);
  }

  private decimal ConvertToUserCurrency(decimal amount, CurrencyEnum fromCurrency, CurrencyEnum toCurrency, List<FinanceApp.Domain.Entities.ExchangeRate> rates)
  {
    if (fromCurrency == toCurrency)
      return Math.Round(amount, 2);
    var rate = rates.FirstOrDefault(r => r.BaseCurrency == fromCurrency.ToString() && r.TargetCurrency == toCurrency.ToString());

    return Math.Round(amount * rate!.Rate, 2);
  }
}
