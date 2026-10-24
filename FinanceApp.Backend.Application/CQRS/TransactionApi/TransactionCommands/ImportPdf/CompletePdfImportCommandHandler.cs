using System.Globalization;
using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Hubs;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.ImportPdf;

public class CompletePdfImportCommandHandler : ICommandHandler<CompletePdfImportCommand, Result<bool>>
{
  private readonly IUserRepository _users;
  private readonly ITransactionRepository _transactions;
  private readonly ITransactionGroupRepository _groups;
  private readonly IExchangeRateService _exchangeRates;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRabbitMqClient _rabbitMq;
  private readonly ISignalRService _signalR;
  private readonly RabbitMqSettings _settings;
  private readonly ILogger<CompletePdfImportCommandHandler> _logger;

  public CompletePdfImportCommandHandler(IUserRepository users, ITransactionRepository transactions,
    ITransactionGroupRepository groups, IExchangeRateService exchangeRates, IUnitOfWork unitOfWork,
    IRabbitMqClient rabbitMq, ISignalRService signalR, IOptions<RabbitMqSettings> settings,
    ILogger<CompletePdfImportCommandHandler> logger)
  {
    _users = users;
    _transactions = transactions;
    _groups = groups;
    _exchangeRates = exchangeRates;
    _unitOfWork = unitOfWork;
    _rabbitMq = rabbitMq;
    _signalR = signalR;
    _settings = settings.Value;
    _logger = logger;
  }

  public async Task<Result<bool>> Handle(CompletePdfImportCommand request, CancellationToken cancellationToken)
  {
    var result = request.Result;
    if (!Guid.TryParse(result.UserId, out var userId) || !Guid.TryParse(result.CorrelationId, out _))
    {
      throw new InvalidDataException("PDF result has invalid identifiers.");
    }

    var user = await _users.GetByIdAsync(userId, noTracking: false, cancellationToken: cancellationToken)
      ?? throw new InvalidDataException("PDF result references an unknown user.");
    if (!result.Success || result.Data is null)
    {
      _logger.LogWarning("PDF extraction failed for correlation {CorrelationId}: {Error}", result.CorrelationId, result.Error);
      await _signalR.SendToClientGroupMethodAsync(user.Email.ToString(), HubConstants.TRANSACTIONS_IMPORT_FAILED_NOTIFICATION,
        JsonSerializer.Serialize(new { correlationId = result.CorrelationId, reason = result.Error ?? "PDF extraction failed." }));
      return Result.Failure<bool>(ApplicationError.LLMProcessorRequestError(result.Error ?? "PDF extraction failed."));
    }

    var transactionsSaved = false;
    try
    {
      ValidateStatement(result.Data);
      var imported = new List<Transaction>(result.Data.Transactions.Count);
      foreach (var item in result.Data.Transactions)
      {
        var amount = decimal.Parse(item.SignedAmount!, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
          CultureInfo.InvariantCulture);
        var currency = Enum.Parse<CurrencyEnum>(item.Currency!, ignoreCase: false);
        var date = DateOnly.ParseExact(item.Date!, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var postedAt = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var absolute = Math.Abs(amount);
        var baseAmount = absolute;
        ConvertedAmount? conversion = null;
        if (currency != CurrencyEnum.EUR)
        {
          var converted = await _exchangeRates.ConvertWithRateAsync(absolute, postedAt, currency.ToString(),
            CurrencyEnum.EUR.ToString(), cancellationToken);
          if (!converted.IsSuccess)
          {
            throw new InvalidDataException($"Exchange rate unavailable for {currency} on {date}.");
          }

          conversion = converted.Data;
          baseAmount = conversion!.Amount;
        }
        var details = item.Details?.Trim() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(item.Reference) && !details.Contains(item.Reference, StringComparison.OrdinalIgnoreCase))
        {
          details = string.IsNullOrWhiteSpace(details) ? item.Reference : $"{details}; reference: {item.Reference}";
        }

        var transaction = new Transaction(item.Description!.Trim(), details,
          amount < 0 ? TransactionTypeEnum.Expense : TransactionTypeEnum.Income,
          new Money { Amount = absolute, Currency = currency }, baseAmount, null, postedAt, user);
        transaction.SetAppliedRate(conversion?.AppliedRate, conversion?.RateValidFrom);
        imported.Add(transaction);
      }

      await _transactions.BatchCreateTransactionsAsync(imported, cancellationToken);
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      transactionsSaved = true;
      await _signalR.SendToClientGroupMethodAsync(user.Email.ToString(), HubConstants.TRANSACTIONS_IMPORTED_NOTIFICATION,
        result.CorrelationId);

      try
      {
        var groups = await _groups.GetAllByUserIdAsync(userId, noTracking: true, cancellationToken: cancellationToken);
        if (groups.Count > 0)
        {
          var job = new MatchTransactionJobDto
          {
            TransactionNames = imported.Select(t => t.Name).Distinct().ToList(),
            TransactionGroupNames = groups.Select(g => g.Name).ToList(),
            UserId = result.UserId,
            CorrelationId = result.CorrelationId
          };
          await _rabbitMq.PublishToQueueAsync(_settings.RequestQueue, JsonSerializer.Serialize(job), cancellationToken);
        }
      }
      catch (Exception error) when (error is not OperationCanceledException)
      {
        _logger.LogError(error, "Transactions were imported, but category matching could not be queued for {CorrelationId}",
          result.CorrelationId);
      }

      return Result.Success(true);
    }
    catch (InvalidDataException error)
    {
      _logger.LogWarning("PDF import rejected for correlation {CorrelationId}: {Reason}", result.CorrelationId, error.Message);
      await _signalR.SendToClientGroupMethodAsync(user.Email.ToString(), HubConstants.TRANSACTIONS_IMPORT_FAILED_NOTIFICATION,
        JsonSerializer.Serialize(new { correlationId = result.CorrelationId, reason = error.Message }));
      return Result.Failure<bool>(ApplicationError.ParsingError());
    }
    catch (Exception error) when (error is not OperationCanceledException)
    {
      _logger.LogError(error, "Unexpected error while processing PDF import {CorrelationId}", result.CorrelationId);
      try
      {
        var notification = transactionsSaved
          ? HubConstants.TRANSACTIONS_IMPORTED_NOTIFICATION
          : HubConstants.TRANSACTIONS_IMPORT_FAILED_NOTIFICATION;
        var message = transactionsSaved
          ? result.CorrelationId
          : JsonSerializer.Serialize(new
          {
            correlationId = result.CorrelationId,
            reason = "The PDF could not be imported because an unexpected server error occurred."
          });
        await _signalR.SendToClientGroupMethodAsync(user.Email.ToString(), notification, message);
      }
      catch (Exception notificationError) when (notificationError is not OperationCanceledException)
      {
        _logger.LogError(notificationError, "Could not notify the client about PDF import {CorrelationId}",
          result.CorrelationId);
      }

      return transactionsSaved
        ? Result.Success(true)
        : Result.Failure<bool>(ApplicationError.ParsingError());
    }
  }

  private static void ValidateStatement(PdfStatementDto statement)
  {
    if (statement.Transactions.Count == 0)
    {
      throw new InvalidDataException("No posted transactions were found in the PDF.");
    }

    if (statement.Transactions.Count > 500)
    {
      throw new InvalidDataException("The statement contains more than 500 transactions.");
    }

    if (!Enum.TryParse<CurrencyEnum>(statement.StatementCurrency, false, out var statementCurrency) ||
        statementCurrency == CurrencyEnum.XXX)
    {
      throw new InvalidDataException("The statement currency is missing or unsupported.");
    }

    decimal debits = 0, credits = 0;
    foreach (var item in statement.Transactions)
    {
      if (string.IsNullOrWhiteSpace(item.Description) || item.Description.Length > 500 ||
          string.IsNullOrWhiteSpace(item.Date) || !DateOnly.TryParseExact(item.Date, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out _) ||
          !Enum.TryParse<CurrencyEnum>(item.Currency, false, out var currency) || currency != statementCurrency ||
          !decimal.TryParse(item.SignedAmount, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
          CultureInfo.InvariantCulture, out var amount) || amount == 0 || amount == decimal.MinValue ||
          Math.Abs(amount) > 99999999999999.9999m || decimal.Round(amount, 4) != amount ||
          item.PageNumber is null or < 1)
      {
        throw new InvalidDataException("The PDF contains an incomplete or unsupported transaction.");
      }

      if (amount < 0)
      {
        debits += Math.Abs(amount);
      }
      else
      {
        credits += amount;
      }
    }

    if (statement.TotalDebits is { } expectedDebits && expectedDebits != debits ||
        statement.TotalCredits is { } expectedCredits && expectedCredits != credits ||
        statement.OpeningBalance is { } opening && statement.ClosingBalance is { } closing &&
        opening + credits - debits != closing)
    {
      throw new InvalidDataException("The extracted amounts do not reconcile with the statement totals.");
    }
  }
}
