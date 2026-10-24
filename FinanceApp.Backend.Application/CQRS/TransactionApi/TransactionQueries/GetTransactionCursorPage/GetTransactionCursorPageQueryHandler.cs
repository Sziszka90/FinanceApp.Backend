using System.Globalization;
using System.Text;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Mappings;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionCursorPage;

public class GetTransactionCursorPageQueryHandler : IQueryHandler<GetTransactionCursorPageQuery, Result<TransactionCursorPageDto>>
{
  private const int DEFAULT_PAGE_SIZE = 50;
  private const int MAXIMUM_PAGE_SIZE = 100;
  private readonly ILogger<GetTransactionCursorPageQueryHandler> _logger;
  private readonly IApplicationMapper _mapper;
  private readonly ITransactionRepository _transactionRepository;
  private readonly IUserService _userService;
  private readonly IExchangeRateService _exchangeRateService;

  public GetTransactionCursorPageQueryHandler(
    ILogger<GetTransactionCursorPageQueryHandler> logger,
    IApplicationMapper mapper,
    ITransactionRepository transactionRepository,
    IUserService userService,
    IExchangeRateService exchangeRateService)
  {
    _logger = logger;
    _mapper = mapper;
    _transactionRepository = transactionRepository;
    _userService = userService;
    _exchangeRateService = exchangeRateService;
  }

  public async Task<Result<TransactionCursorPageDto>> Handle(GetTransactionCursorPageQuery request, CancellationToken cancellationToken)
  {
    var pageSize = request.PageSize == 0 ? DEFAULT_PAGE_SIZE : request.PageSize;
    if (pageSize < 1 || pageSize > MAXIMUM_PAGE_SIZE)
    {
      return InvalidRequest(nameof(request.PageSize), $"Page size must be between 1 and {MAXIMUM_PAGE_SIZE}.");
    }

    DateTimeOffset? cursorDate = null;
    Guid? cursorId = null;
    if (!string.IsNullOrWhiteSpace(request.Cursor) && !TryDecodeCursor(request.Cursor, out cursorDate, out cursorId))
    {
      return InvalidRequest(nameof(request.Cursor), "The transaction cursor is invalid.");
    }

    var user = await _userService.GetActiveUserAsync(cancellationToken);
    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<TransactionCursorPageDto>(user.ApplicationError!);
    }

    var transactions = await _transactionRepository.GetCursorPageAsync(
      request.TransactionFilter,
      cursorDate,
      cursorId,
      pageSize + 1,
      cancellationToken);

    var hasMore = transactions.Count > pageSize;
    if (hasMore)
    {
      transactions.RemoveAt(transactions.Count - 1);
    }

    foreach (var transaction in transactions)
    {
      if (transaction.Value.Currency == user.Data!.BaseCurrency)
      {
        continue;
      }

      if (user.Data.BaseCurrency == CurrencyEnum.EUR)
      {
        transaction.Value.Amount = transaction.ValueInBaseCurrency;
        transaction.Value.Currency = CurrencyEnum.EUR;
        continue;
      }

      var converted = await _exchangeRateService.ConvertAmountAsync(
        transaction.Value.Amount,
        transaction.TransactionDate,
        transaction.Value.Currency.ToString(),
        user.Data.BaseCurrency.ToString(),
        cancellationToken);

      if (!converted.IsSuccess)
      {
        _logger.LogWarning("Failed to convert transaction {TransactionId}: {Error}", transaction.Id, converted.ApplicationError?.Message);
        return Result.Failure<TransactionCursorPageDto>(converted.ApplicationError!);
      }

      transaction.Value.Amount = converted.Data;
      transaction.Value.Currency = user.Data.BaseCurrency;
    }

    var result = new TransactionCursorPageDto
    {
      Items = _mapper.ToGetTransactionDtos(transactions),
      NextCursor = hasMore && transactions.Count > 0 ? EncodeCursor(transactions[^1]) : null
    };

    return Result.Success(result);
  }

  private static Result<TransactionCursorPageDto> InvalidRequest(string propertyName, string message)
  {
    var failure = new ValidationFailure(propertyName, message);
    return Result.Failure<TransactionCursorPageDto>(ApplicationError.ValidationError([failure]));
  }

  private static string EncodeCursor(Transaction transaction)
  {
    var value = $"{transaction.TransactionDate.ToString("O", CultureInfo.InvariantCulture)}|{transaction.Id:D}";
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
  }

  private static bool TryDecodeCursor(string cursor, out DateTimeOffset? transactionDate, out Guid? transactionId)
  {
    transactionDate = null;
    transactionId = null;
    try
    {
      var parts = Encoding.UTF8.GetString(Convert.FromBase64String(cursor)).Split('|', 2);
      if (parts.Length != 2 ||
          !DateTimeOffset.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
          !Guid.TryParse(parts[1], out var id))
      {
        return false;
      }

      transactionDate = date;
      transactionId = id;
      return true;
    }
    catch (FormatException)
    {
      return false;
    }
  }
}
