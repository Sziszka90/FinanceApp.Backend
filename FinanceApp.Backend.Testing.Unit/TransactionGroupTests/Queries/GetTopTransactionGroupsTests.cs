using FinanceApp.Backend.Application.Abstraction.Services;
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
  private readonly Mock<IUserService> _userServiceMock;
  private readonly GetTopTransactionGroupsQueryHandler _handler;

  public GetTopTransactionGroupsTests()
  {
    _loggerMock = CreateLoggerMock<GetTopTransactionGroupsQueryHandler>();
    _userServiceMock = new Mock<IUserService>();
    _handler = new GetTopTransactionGroupsQueryHandler(
      _loggerMock.Object,
      TransactionRepositoryMock.Object,
      UserRepositoryMock.Object,
      ExchangeRateRepositoryMock.Object,
      _userServiceMock.Object
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

    var aggregatedData = new List<TransactionGroupAggregate>
    {
      new TransactionGroupAggregate
      {
        TransactionGroup = transactionGroup1,
        Currency = CurrencyEnum.USD,
        TotalAmount = 500.00m,
        TransactionCount = 10
      },
      new TransactionGroupAggregate
      {
        TransactionGroup = transactionGroup2,
        Currency = CurrencyEnum.USD,
        TotalAmount = 300.00m,
        TransactionCount = 5
      }
    };

    TransactionRepositoryMock
      .Setup(x => x.GetTransactionGroupAggregatesAsync(
        It.IsAny<Guid>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<int>(),
        true,
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(aggregatedData);



    var exchangeRates = new List<ExchangeRate>();

    _userServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    TransactionRepositoryMock
      .Setup(x => x.GetTransactionGroupAggregatesAsync(
        userId,
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<int>(),
        true,
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(aggregatedData);

    ExchangeRateRepositoryMock
      .Setup(x => x.GetExchangeRatesAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(exchangeRates);

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      10
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
    Assert.Equal(10, result.Data[0].TransactionCount);

    // Second should be Transport
    Assert.Equal("Transport", result.Data[1].Name);
    Assert.Equal(300.00m, result.Data[1].TotalAmount.Amount);
    Assert.Equal(5, result.Data[1].TransactionCount);

    // Verify all dependencies were called
    _userServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.GetTransactionGroupAggregatesAsync(
      It.IsAny<Guid>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<int>(),
      true,
      It.IsAny<CancellationToken>()), Times.Once);
    ExchangeRateRepositoryMock.Verify(x => x.GetExchangeRatesAsync(true, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task Handle_UserServiceFails_ReturnsFailure()
  {
    // Arrange
    var userError = ApplicationError.UserNotLoggedInError();
    var userResult = Result.Failure<User>(userError);

    _userServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      10
    );

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(userError, result.ApplicationError);

    // Verify repository methods were not called
    TransactionRepositoryMock.Verify(x => x.GetTransactionGroupAggregatesAsync(
      It.IsAny<Guid>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<int>(),
      It.IsAny<bool>(),
      It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_NoTransactionGroups_ReturnsEmptyList()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hashedPassword", CurrencyEnum.USD);
    var userResult = Result.Success(user);

    _userServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    TransactionRepositoryMock
      .Setup(x => x.GetTransactionGroupAggregatesAsync(
        It.IsAny<Guid>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<int>(),
        true,
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroupAggregate>());

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      10
    );

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Empty(result.Data);

    // Verify user service was called but exchange rates were not (optimization)
    _userServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    ExchangeRateRepositoryMock.Verify(x => x.GetExchangeRatesAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_MultiCurrencyTransactions_ConvertsCorrectly()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hashedPassword", CurrencyEnum.USD);
    var userResult = Result.Success(user);

    var transactionGroup = new TransactionGroup("Shopping", "Shopping expenses", "🛒", user);

    var aggregatedData = new List<TransactionGroupAggregate>
    {
      new TransactionGroupAggregate
      {
        TransactionGroup = transactionGroup,
        Currency = CurrencyEnum.USD,
        TotalAmount = 100.00m,
        TransactionCount = 2
      },
      new TransactionGroupAggregate
      {
        TransactionGroup = transactionGroup,
        Currency = CurrencyEnum.EUR,
        TotalAmount = 50.00m, // This should be converted to USD
        TransactionCount = 3
      }
    };

    // Mock exchange rates (1 EUR = 1.1 USD)
    var exchangeRates = new List<ExchangeRate>
    {
      new ExchangeRate(CurrencyEnum.EUR.ToString(), CurrencyEnum.USD.ToString(), 1.1m)
    };

    _userServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    TransactionRepositoryMock
      .Setup(x => x.GetTransactionGroupAggregatesAsync(
        It.IsAny<Guid>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<int>(),
        true,
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(aggregatedData);

    ExchangeRateRepositoryMock
      .Setup(x => x.GetExchangeRatesAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(exchangeRates);

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      10
    );

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Single(result.Data);

    var resultGroup = result.Data[0];
    Assert.Equal("Shopping", resultGroup.Name);
    Assert.Equal(CurrencyEnum.USD, resultGroup.TotalAmount.Currency);
    Assert.Equal(5, resultGroup.TransactionCount); // 2 + 3

    // Total should be 100 USD + (50 EUR * 1.1) = 155 USD
    Assert.Equal(155.00m, resultGroup.TotalAmount.Amount);
  }

  [Fact]
  public async Task Handle_RespectsTopLimit()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hashedPassword", CurrencyEnum.USD);
    var userResult = Result.Success(user);

    var exchangeRates = new List<ExchangeRate>();
    var topLimit = 2;

    _userServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(userResult);

    // Mock repository to return exactly the top limit (simulating database-level limiting)
    TransactionRepositoryMock
      .Setup(x => x.GetTransactionGroupAggregatesAsync(
        It.IsAny<Guid>(),
        It.IsAny<DateTimeOffset>(),
        It.IsAny<DateTimeOffset>(),
        topLimit,
        true,
        It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroupAggregate>
      {
        new TransactionGroupAggregate
        {
          TransactionGroup = new TransactionGroup("Group1", "Description1", "🏷️", user),
          Currency = CurrencyEnum.USD,
          TotalAmount = 1000.00m,
          TransactionCount = 10
        },
        new TransactionGroupAggregate
        {
          TransactionGroup = new TransactionGroup("Group2", "Description2", "🏷️", user),
          Currency = CurrencyEnum.USD,
          TotalAmount = 800.00m,
          TransactionCount = 8
        }
      });

    ExchangeRateRepositoryMock
      .Setup(x => x.GetExchangeRatesAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(exchangeRates);

    var query = new GetTopTransactionGroupsQuery(
      DateTimeOffset.Now.AddDays(-30),
      DateTimeOffset.Now,
      topLimit
    );

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(topLimit, result.Data.Count);

    // Verify the repository was called with the correct top limit
    TransactionRepositoryMock.Verify(x => x.GetTransactionGroupAggregatesAsync(
      It.IsAny<Guid>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<DateTimeOffset>(),
      topLimit,
      true,
      It.IsAny<CancellationToken>()), Times.Once);
  }
}
