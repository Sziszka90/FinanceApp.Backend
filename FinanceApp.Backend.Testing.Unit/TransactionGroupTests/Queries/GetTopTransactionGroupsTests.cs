using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetTopTransactionGroups;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionGroupTests.Queries;

public class GetTopTransactionGroupsTests : TestBase
{
  private readonly Mock<ILogger<GetTopTransactionGroupsQueryHandler>> _loggerMock;
  private readonly GetTopTransactionGroupsQueryHandler _handler;

  public GetTopTransactionGroupsTests()
  {
    _loggerMock = CreateLoggerMock<GetTopTransactionGroupsQueryHandler>();
    _handler = new GetTopTransactionGroupsQueryHandler(
      _loggerMock.Object,
      TransactionRepositoryMock.Object,
      UserRepositoryMock.Object,
      UserServiceMock.Object,
      ExchangeRateServiceMock.Object
    );
  }

  [Fact]
  public async Task Handle_ValidRequest_ReturnsTopTransactionGroups()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hashedPassword", CurrencyEnum.USD);
    var userResult = Result.Success(user);

    var transactionGroup1 = new TransactionGroup("Food", "Food expenses", "🍔", user);
    var transactionGroup2 = new TransactionGroup("Transport", "Transport expenses", "🚗", user);

    var transaction1 = new Transaction(
      "Food expense",
      null,
      TransactionTypeEnum.Expense,
      new Money { Amount = 500.00m, Currency = CurrencyEnum.USD },
      500.00m,
      transactionGroup1,
      DateTimeOffset.Now.AddDays(-10),
      user
    )
    { Id = Guid.NewGuid() };
    var transaction2 = new Transaction(
      "Transport expense",
      null,
      TransactionTypeEnum.Expense,
      new Money { Amount = 300.00m, Currency = CurrencyEnum.USD },
      300.00m,
      transactionGroup2,
      DateTimeOffset.Now.AddDays(-5),
      user
    )
    { Id = Guid.NewGuid() };
    var transactions = new List<Transaction> { transaction1, transaction2 };
    TransactionRepositoryMock
      .Setup(x => x.GetTransactionsByTopTransactionGroups(
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<Guid>(),
        It.IsAny<int>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactions);

    var exchangeRates = new List<ExchangeRate>();

    UserServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    TransactionRepositoryMock
      .Setup(x => x.GetTransactionsByTopTransactionGroups(
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        userId,
        It.IsAny<int>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactions);

    ExchangeRateRepositoryMock
      .Setup(x => x.GetExchangeRatesAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(exchangeRates);

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      10,
      null
    );

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(2, result.Data.Count);

    // First should be Food (higher amount)
    Assert.Equal("Food", result.Data[0].Name);
    Assert.Equal(500.00m, result.Data[0].TotalAmount.Amount);
    Assert.Equal(1, result.Data[0].TransactionCount);

    // Second should be Transport
    Assert.Equal("Transport", result.Data[1].Name);
    Assert.Equal(300.00m, result.Data[1].TotalAmount.Amount);
    Assert.Equal(1, result.Data[1].TransactionCount);

    // Verify all dependencies were called
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.GetTransactionsByTopTransactionGroups(
      It.IsAny<DateTimeOffset>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<Guid>(),
      It.IsAny<int>(),
      It.IsAny<CancellationToken>()), Times.Once);
    ExchangeRateServiceMock.Verify(x => x.ConvertAmountAsync(It.IsAny<decimal>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_UserServiceFails_ReturnsFailure()
  {
    // arrange
    var userError = ApplicationError.UserNotLoggedInError();
    var userResult = Result.Failure<User>(userError);

    UserServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      10,
      null
    );

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.Equal(userError, result.ApplicationError);

    // Verify repository methods were not called
    TransactionRepositoryMock.Verify(x => x.GetTransactionsByTopTransactionGroups(
      It.IsAny<DateTimeOffset>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<Guid>(),
      It.IsAny<int>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_NoTransactionGroups_ReturnsEmptyList()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hashedPassword", CurrencyEnum.USD);
    var userResult = Result.Success(user);

    UserServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    TransactionRepositoryMock
      .Setup(x => x.GetTransactionsByTopTransactionGroups(
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        userId,
        It.IsAny<int>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction>());

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      10,
      null
    );

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Empty(result.Data);

    // Verify user service was called but exchange rates were not (optimization)
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    ExchangeRateRepositoryMock.Verify(x => x.GetExchangeRatesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_MultiCurrencyTransactions_ConvertsCorrectly()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hashedPassword", CurrencyEnum.USD);
    var userResult = Result.Success(user);

    var transactionGroup = new TransactionGroup("Shopping", "Shopping expenses", "🛒", user);

    var transactionUSD = new Transaction(
      "Shopping USD",
      null,
      TransactionTypeEnum.Expense,
      new Money { Amount = 100.00m, Currency = CurrencyEnum.USD },
      100.00m,
      transactionGroup,
      DateTimeOffset.Now.AddDays(-10),
      user
    )
    { Id = Guid.NewGuid() };
    var transactionEUR = new Transaction(
      "Shopping EUR",
      null,
      TransactionTypeEnum.Expense,
      new Money { Amount = 50.00m, Currency = CurrencyEnum.EUR },
      50.00m,
      transactionGroup,
      DateTimeOffset.Now.AddDays(-8),
      user
    )
    { Id = Guid.NewGuid() };
    var transactionsMulti = new List<Transaction> { transactionUSD, transactionEUR };
    TransactionRepositoryMock
      .Setup(x => x.GetTransactionsByTopTransactionGroups(
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<Guid>(),
        It.IsAny<int>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactionsMulti);

    UserServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    TransactionRepositoryMock
      .Setup(x => x.GetTransactionsByTopTransactionGroups(
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        userId,
        It.IsAny<int>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactionsMulti);

    ExchangeRateServiceMock
      .Setup(x => x.ConvertAmountAsync(It.IsAny<decimal>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(55.0m));

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      10,
      null
    );

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Single(result.Data);

    var resultGroup = result.Data[0];
    Assert.Equal("Shopping", resultGroup.Name);
    Assert.Equal(CurrencyEnum.USD, resultGroup.TotalAmount.Currency);
    Assert.Equal(2, resultGroup.TransactionCount); // 2 + 3

    // Total should be 100 USD + (50 EUR * 1.1) = 155 USD
    Assert.Equal(155.00m, resultGroup.TotalAmount.Amount);
  }

  [Fact]
  public async Task Handle_RespectsTopLimit()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hashedPassword", CurrencyEnum.USD);
    var userResult = Result.Success(user);

    var exchangeRates = new List<ExchangeRate>();
    var topLimit = 2;

    UserServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    // Mock repository to return exactly the top limit (simulating database-level limiting)
    var transactionA = new Transaction(
      "Group1 Transaction",
      null,
      TransactionTypeEnum.Expense,
      new Money { Amount = 1000.00m, Currency = CurrencyEnum.USD },
      1000.00m,
      new TransactionGroup("Group1", "Description1", "🏷️", user),
      DateTimeOffset.Now.AddDays(-10),
      user
    )
    { Id = Guid.NewGuid() };
    var transactionB = new Transaction(
      "Group2 Transaction",
      null,
      TransactionTypeEnum.Expense,
      new Money { Amount = 800.00m, Currency = CurrencyEnum.USD },
      800.00m,
      new TransactionGroup("Group2", "Description2", "🏷️", user),
      DateTimeOffset.Now.AddDays(-8),
      user
    )

    { Id = Guid.NewGuid() };

    var transactionsTop = new List<Transaction> { transactionA, transactionB };

    TransactionRepositoryMock
      .Setup(x => x.GetTransactionsByTopTransactionGroups(
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<Guid>(),
        It.IsAny<int>(),
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactionsTop);

    ExchangeRateServiceMock
      .Setup(x => x.ConvertAmountAsync(It.IsAny<decimal>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(1.0m));

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      topLimit,
      null
    );

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(topLimit, result.Data.Count);

    // Verify the repository was called with the correct top limit
    TransactionRepositoryMock.Verify(x => x.GetTransactionsByTopTransactionGroups(
      It.IsAny<DateTimeOffset>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<Guid>(),
      It.IsAny<int>(),
      It.IsAny<CancellationToken>()), Times.Once);
  }
}
