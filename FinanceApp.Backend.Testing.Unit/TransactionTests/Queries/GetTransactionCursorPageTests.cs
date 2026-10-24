using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionCursorPage;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Queries;

public class GetTransactionCursorPageTests : TestBase
{
  private readonly Mock<ILogger<GetTransactionCursorPageQueryHandler>> _logger = CreateLoggerMock<GetTransactionCursorPageQueryHandler>();
  private readonly User _user = new(null, "test-user", "test@example.com", true, "hash", CurrencyEnum.EUR);
  private readonly GetTransactionCursorPageQueryHandler _handler;

  public GetTransactionCursorPageTests()
  {
    UserServiceMock.Setup(service => service.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result<User>.Success(_user));
    _handler = new GetTransactionCursorPageQueryHandler(
      _logger.Object,
      Mapper,
      TransactionRepositoryMock.Object,
      UserServiceMock.Object,
      ExchangeRateServiceMock.Object);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(101)]
  public async Task Handle_InvalidPageSize_ReturnsValidationFailure(int pageSize)
  {
    var result = await _handler.Handle(new GetTransactionCursorPageQuery(pageSize, null, null, CancellationToken.None), CancellationToken.None);

    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    TransactionRepositoryMock.Verify(repository => repository.GetCursorPageAsync(
      It.IsAny<FinanceApp.Backend.Application.Dtos.TransactionDtos.TransactionFilter?>(),
      It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Theory]
  [InlineData("not-base64")]
  [InlineData("dG9vLWZldy1wYXJ0cw==")]
  [InlineData("bm90LWFuLWlkfG5vdC1hLWRhdGU=")]
  public async Task Handle_InvalidCursor_ReturnsValidationFailure(string cursor)
  {
    var result = await _handler.Handle(new GetTransactionCursorPageQuery(10, cursor, null, CancellationToken.None), CancellationToken.None);

    Assert.False(result.IsSuccess);
    TransactionRepositoryMock.Verify(repository => repository.GetCursorPageAsync(
      It.IsAny<FinanceApp.Backend.Application.Dtos.TransactionDtos.TransactionFilter?>(),
      It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_ActiveUserFailure_ReturnsFailureWithoutQueryingTransactions()
  {
    UserServiceMock.Setup(service => service.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Failure<User>(ApplicationError.UserNotFoundError()));

    var result = await _handler.Handle(new GetTransactionCursorPageQuery(10, null, null, CancellationToken.None), CancellationToken.None);

    Assert.False(result.IsSuccess);
    TransactionRepositoryMock.Verify(repository => repository.GetCursorPageAsync(
      It.IsAny<FinanceApp.Backend.Application.Dtos.TransactionDtos.TransactionFilter?>(),
      It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_ZeroPageSizeUsesDefaultAndReturnsEmptyPage()
  {
    int? requestedPageSize = null;
    TransactionRepositoryMock.Setup(repository => repository.GetCursorPageAsync(
        It.IsAny<FinanceApp.Backend.Application.Dtos.TransactionDtos.TransactionFilter?>(),
        null, null, It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .Callback<FinanceApp.Backend.Application.Dtos.TransactionDtos.TransactionFilter?, DateTimeOffset?, Guid?, int, CancellationToken>(
        (_, _, _, pageSize, _) => requestedPageSize = pageSize)
      .ReturnsAsync(new List<Transaction>());

    var result = await _handler.Handle(new GetTransactionCursorPageQuery(0, null, null, CancellationToken.None), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Empty(result.Data!.Items);
    Assert.Null(result.Data.NextCursor);
    Assert.Equal(51, requestedPageSize);
  }

  [Fact]
  public async Task Handle_HasMoreTransactions_ReturnsNextCursorFromLastIncludedItem()
  {
    var first = CreateTransaction("First", new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero));
    var extra = CreateTransaction("Extra", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
    TransactionRepositoryMock.Setup(repository => repository.GetCursorPageAsync(
        It.IsAny<FinanceApp.Backend.Application.Dtos.TransactionDtos.TransactionFilter?>(),
        null, null, 2, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { first, extra });

    var result = await _handler.Handle(new GetTransactionCursorPageQuery(1, null, null, CancellationToken.None), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Single(result.Data!.Items);
    Assert.Equal("First", result.Data.Items[0].Name);
    Assert.NotNull(result.Data.NextCursor);
    var decodedCursor = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(result.Data.NextCursor));
    Assert.Equal($"{first.TransactionDate:O}|{first.Id:D}", decodedCursor);
  }

  [Fact]
  public async Task Handle_CursorIsDecodedAndPassedToRepository()
  {
    var cursorDate = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);
    var cursorId = Guid.NewGuid();
    var cursor = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{cursorDate:O}|{cursorId:D}"));
    DateTimeOffset? capturedDate = null;
    Guid? capturedId = null;
    TransactionRepositoryMock.Setup(repository => repository.GetCursorPageAsync(
        It.IsAny<FinanceApp.Backend.Application.Dtos.TransactionDtos.TransactionFilter?>(),
        It.IsAny<DateTimeOffset?>(), It.IsAny<Guid?>(), 11, It.IsAny<CancellationToken>()))
      .Callback<FinanceApp.Backend.Application.Dtos.TransactionDtos.TransactionFilter?, DateTimeOffset?, Guid?, int, CancellationToken>(
        (_, date, id, _, _) => { capturedDate = date; capturedId = id; })
      .ReturnsAsync(new List<Transaction>());

    var result = await _handler.Handle(new GetTransactionCursorPageQuery(10, cursor, null, CancellationToken.None), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(cursorDate, capturedDate);
    Assert.Equal(cursorId, capturedId);
  }

  [Fact]
  public async Task Handle_ConvertsItemToEuroForEuroBaseCurrency()
  {
    var transaction = CreateTransaction("Card purchase", DateTimeOffset.UtcNow, CurrencyEnum.USD, 20m, 18m);
    TransactionRepositoryMock.Setup(repository => repository.GetCursorPageAsync(
        It.IsAny<FinanceApp.Backend.Application.Dtos.TransactionDtos.TransactionFilter?>(),
        null, null, 6, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { transaction });

    var result = await _handler.Handle(new GetTransactionCursorPageQuery(5, null, null, CancellationToken.None), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(18m, result.Data!.Items[0].Value.Amount);
    Assert.Equal(CurrencyEnum.EUR, result.Data.Items[0].Value.Currency);
    ExchangeRateServiceMock.Verify(service => service.ConvertAmountAsync(It.IsAny<decimal>(), It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  private Transaction CreateTransaction(string name, DateTimeOffset date, CurrencyEnum currency = CurrencyEnum.EUR,
    decimal amount = 10m, decimal amountInEur = 10m)
  {
    return new Transaction(name, "Test transaction", TransactionTypeEnum.Expense,
      new Money { Amount = amount, Currency = currency }, amountInEur, null, date, _user);
  }
}
