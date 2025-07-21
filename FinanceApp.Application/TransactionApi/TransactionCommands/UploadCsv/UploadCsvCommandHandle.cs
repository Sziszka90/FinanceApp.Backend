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
  private readonly IUnitOfWork _unitOfWork;
  private readonly ILLMProcessorClient _llmProcessorClient;

  public UploadCsvCommandHandler(
    ILogger<UploadCsvCommandHandler> logger,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository,
    ITransactionRepository transactionRepository,
    ITransactionGroupRepository transactionGroupRepository,
    IUnitOfWork unitOfWork,
    ILLMProcessorClient llmProcessorClient)
  {
    _logger = logger;
    _mapper = mapper;
    _httpContextAccessor = httpContextAccessor;
    _userRepository = userRepository;
    _transactionRepository = transactionRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
    _llmProcessorClient = llmProcessorClient;
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

    await _transactionRepository.BatchCreateTransactionsAsync(transactions, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    var existingTransactionGroups = await _transactionGroupRepository.GetAllAsync(false, cancellationToken: cancellationToken);

    var llmProcessResult = await _llmProcessorClient.MatchTransactionGroup(
      transactions.Select(t => t.Name).ToList(),
      existingTransactionGroups.Select(g => g.Name).ToList(),
      user.Id.ToString(),
      request.uploadCsvFileDto.CorrelationId
    );

    if (!llmProcessResult.IsSuccess)
    {
      _logger.LogError("Failed to match transaction groups: {ErrorMessage}", llmProcessResult.ApplicationError?.Message);
      return Result.Failure<List<GetTransactionDto>>(ApplicationError.LLMProcessorRequestError(llmProcessResult.ApplicationError!.Message!));
    }

    var allTransactions = await _transactionRepository.GetAllAsync(noTracking: true, cancellationToken: cancellationToken);

    _logger.LogDebug("CSV file uploaded and transactions created for user: {UserId}", user.Id);
    return Result.Success(_mapper.Map<List<GetTransactionDto>>(allTransactions));
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
