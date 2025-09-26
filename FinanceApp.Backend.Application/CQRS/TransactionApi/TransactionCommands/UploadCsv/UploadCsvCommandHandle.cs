using System.Globalization;
using System.Text.RegularExpressions;
using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UploadCsv;

public class UploadCsvCommandHandler : ICommandHandler<UploadCsvCommand, Result<List<GetTransactionDto>>>
{
  private readonly ILogger<UploadCsvCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly ITransactionRepository _transactionRepository;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserService _userService;
  private readonly ILLMProcessorClient _llmProcessorClient;
  private readonly IExchangeRateService _exchangeRateService;

  public UploadCsvCommandHandler(
    ILogger<UploadCsvCommandHandler> logger,
    IMapper mapper,
    ITransactionRepository transactionRepository,
    ITransactionGroupRepository transactionGroupRepository,
    IUnitOfWork unitOfWork,
    IUserService userService,
    ILLMProcessorClient llmProcessorClient,
    IExchangeRateService exchangeRateService)
  {
    _logger = logger;
    _mapper = mapper;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
    _userService = userService;
    _llmProcessorClient = llmProcessorClient;
    _exchangeRateService = exchangeRateService;
  }

  /// <inheritdoc />
  public async Task<Result<List<GetTransactionDto>>> Handle(UploadCsvCommand request, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<List<GetTransactionDto>>(user.ApplicationError!);
    }

    var transactions = await ImportTransactions(request.uploadCsvFileDto.File, user.Data!, cancellationToken);

    await _transactionRepository.BatchCreateTransactionsAsync(transactions, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    var existingTransactionGroups = await _transactionGroupRepository.GetAllAsync(false, cancellationToken: cancellationToken);

    var llmProcessResult = await _llmProcessorClient.MatchTransactionGroup(
      user.Data!.Id.ToString(),
      transactions.Select(t => t.Name).ToList(),
      existingTransactionGroups.Select(g => g.Name).ToList(),
      request.uploadCsvFileDto.CorrelationId
    );

    if (!llmProcessResult.IsSuccess)
    {
      _logger.LogError("Failed to match transaction groups: {ErrorMessage}", llmProcessResult.ApplicationError?.Message);
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.LLMProcessorRequestError(llmProcessResult.ApplicationError!.Message!));
    }

    var allTransactions = await _transactionRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);

    _logger.LogInformation("CSV file uploaded and transactions created for user: {UserId}", user.Data!.Id);
    return Result.Success(_mapper.Map<List<GetTransactionDto>>(allTransactions));
  }

  private string CleanCsvField(string input)
  {
    if (string.IsNullOrEmpty(input))
    {
      return input;
    }

    var cleaned = input.Trim();
    if (cleaned.StartsWith("\"") && cleaned.EndsWith("\""))
    {
      cleaned = cleaned.Substring(1, cleaned.Length - 2);
    }

    cleaned = cleaned.Replace("\\", "");
    return cleaned;
  }

  private async Task<List<Transaction>> ImportTransactions(IFormFile file, User user, CancellationToken cancellationToken)
  {
    var transactions = new List<Transaction>();

    using (var stream = file.OpenReadStream())
    using (var reader = new StreamReader(stream))
    {
      string? headerLine = await reader.ReadLineAsync();
      while (!reader.EndOfStream)
      {
        var line = await reader.ReadLineAsync();
        if (string.IsNullOrWhiteSpace(line))
        {
          continue;
        }

        var columns = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

        var cleanedAmount = CleanCsvField(RemoveSpaces(columns[3]));

        var normalizedAmount = cleanedAmount.Replace(',', '.');
        var amount = decimal.TryParse(normalizedAmount, NumberStyles.Number | NumberStyles.AllowThousands,
          CultureInfo.InvariantCulture, out var parsedAmount) ? parsedAmount : 0;

        var transactionDate = DateTimeOffset.TryParse(
          CleanCsvField(columns[2]),
          CultureInfo.InvariantCulture,
          DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
          out var date) ? date : DateTimeOffset.UtcNow;

        var currencyResult = Enum.TryParse<CurrencyEnum>(CleanCsvField(columns[4]), out var currency) ? currency : CurrencyEnum.UNKNOWN;

        decimal valueInBaseCurrency = Math.Abs(amount);
        if (currencyResult != CurrencyEnum.EUR && currencyResult != CurrencyEnum.UNKNOWN)
        {
          var valueInBaseCurrencyResult = await _exchangeRateService.ConvertAmountAsync(
            Math.Abs(amount),
            transactionDate,
            currencyResult.ToString(),
            CurrencyEnum.EUR.ToString(),
            cancellationToken);

          if (!valueInBaseCurrencyResult.IsSuccess)
          {
            _logger.LogError("Failed to convert amount: {Error}", valueInBaseCurrencyResult.ApplicationError?.Message);
            continue;
          }
          valueInBaseCurrency = valueInBaseCurrencyResult.Data;
        }

        if (currencyResult == CurrencyEnum.UNKNOWN)
        {
          _logger.LogWarning("Unknown currency for transaction: {Line}", line);
        }

        var transaction = new Transaction(
          NormalizeSpaces(CleanCsvField(columns[5]) != "" ? CleanCsvField(columns[5]) : "Unknown"),
          CleanCsvField(columns[9]),
          amount < 0 ? TransactionTypeEnum.Expense : TransactionTypeEnum.Income,
          new Money
          {
            Amount = Math.Abs(amount),
            Currency = currencyResult
          },
          valueInBaseCurrency,
          null,
          transactionDate,
          user!
        );
        transactions.Add(transaction);
      }
    }
    return transactions;
  }

  private string NormalizeSpaces(string input)
  {
    if (string.IsNullOrWhiteSpace(input))
    {
      return string.Empty;
    }

    return Regex.Replace(input.Trim(), @"\s+", " ");
  }

  private string RemoveSpaces(string input)
  {
    if (string.IsNullOrEmpty(input))
    {
      return input;
    }
    return Regex.Replace(input, @"\s+", "");
  }
}
