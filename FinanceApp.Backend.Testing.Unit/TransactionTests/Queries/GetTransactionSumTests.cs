using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionSum;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Queries;

public class GetTransactionSumTests : TestBase
{
  private readonly Mock<ILogger<GetTransactionSumQueryHandler>> _loggerMock;
  private readonly GetTransactionSumQueryHandler _handler;

  public GetTransactionSumTests()
  {
    _loggerMock = CreateLoggerMock<GetTransactionSumQueryHandler>();
    _handler = new GetTransactionSumQueryHandler(
        _loggerMock.Object,
        TransactionRepositoryMock.Object,
        ExchangeRateRepositoryMock.Object,
        UserServiceMock.Object
    );
  }

  [Fact]
  public async Task Handle_ValidUserWithSameCurrencyTransactions_ReturnsCorrectSum()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var transactionGroup = new TransactionGroup("Test Group", "Description", "", user);

    var transactions = new List<Transaction>
    {
      new Transaction("Income 1", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 100, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user),
      new Transaction("Expense 1", "Description", TransactionTypeEnum.Expense,
        new Money { Amount = 50, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user),
      new Transaction("Income 2", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 200, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user)
    };

    UserRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User> { user });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactions);

    var query = new GetTransactionSumQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(250, result.Data!.Amount); // 100 + 200 - 50
    Assert.Equal(CurrencyEnum.USD, result.Data.Currency);
  }

  [Fact]
  public async Task Handle_ValidUserWithMixedCurrencyTransactions_ReturnsCorrectSumWithConversion()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var transactionGroup = new TransactionGroup("Test Group", "Description", "", user);

    var transactions = new List<Transaction>
    {
      new Transaction("Income USD", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 100, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user),
      new Transaction("Income EUR", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 100, Currency = CurrencyEnum.EUR }, transactionGroup, DateTime.UtcNow, user), // Should be 100 * 1.18 = 118
      new Transaction("Expense USD", "Description", TransactionTypeEnum.Expense,
        new Money { Amount = 50, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user)
    };

    UserRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User> { user });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactions);

    var query = new GetTransactionSumQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(168, result.Data!.Amount); // 100 - 50 + (100 * 1.18) = 168
    Assert.Equal(CurrencyEnum.USD, result.Data.Currency);
  }

  [Fact]
  public async Task Handle_UserNotFound_ReturnsFailureResult()
  {
    // arrange
    UserServiceMock
      .Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Failure<User>(ApplicationError.UserNotFoundError()));

    var query = new GetTransactionSumQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError!.Code);
  }

  [Fact]
  public async Task Handle_EmptyTransactionsList_ReturnsZeroSum()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User> { user });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction>());

    var query = new GetTransactionSumQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(0, result.Data!.Amount);
    Assert.Equal(CurrencyEnum.USD, result.Data.Currency);
  }

  [Fact]
  public async Task Handle_OnlyExpenseTransactions_ReturnsNegativeSum()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var transactionGroup = new TransactionGroup("Test Group", "Description", "", user);

    var transactions = new List<Transaction>
    {
      new Transaction("Expense 1", "Description", TransactionTypeEnum.Expense,
        new Money { Amount = 75.50m, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user),
      new Transaction("Expense 2", "Description", TransactionTypeEnum.Expense,
        new Money { Amount = 125.75m, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user)
    };

    UserRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User> { user });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactions);

    var query = new GetTransactionSumQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(-201.25m, result.Data!.Amount); // -(75.50 + 125.75)
    Assert.Equal(CurrencyEnum.USD, result.Data.Currency);
  }

  [Fact]
  public async Task Handle_OnlyIncomeTransactions_ReturnsPositiveSum()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var transactionGroup = new TransactionGroup("Test Group", "Description", "", user);

    var transactions = new List<Transaction>
    {
      new Transaction("Income 1", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 500.25m, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user),
      new Transaction("Income 2", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 750.50m, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user)
    };

    UserRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User> { user });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactions);

    var query = new GetTransactionSumQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(1250.75m, result.Data!.Amount); // 500.25 + 750.50
    Assert.Equal(CurrencyEnum.USD, result.Data.Currency);
  }

  [Fact]
  public async Task Handle_ComplexMixedCurrencyScenario_ReturnsCorrectSum()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var transactionGroup = new TransactionGroup("Test Group", "Description", "", user);

    var transactions = new List<Transaction>
    {
      new Transaction("Income USD", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 1000, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user),
      new Transaction("Income EUR", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 500, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user), // 500 * 1.18 = 590
      new Transaction("Expense GBP", "Description", TransactionTypeEnum.Expense,
        new Money { Amount = 200, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user), // 200 * 1.33 = 266
      new Transaction("Expense USD", "Description", TransactionTypeEnum.Expense,
        new Money { Amount = 300, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user)
    };

    UserRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User> { user });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactions);

    var query = new GetTransactionSumQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(1000, result.Data!.Amount);
    Assert.Equal(CurrencyEnum.USD, result.Data.Currency);
  }

  [Fact]
  public async Task Handle_AmountRounding_ReturnsRoundedResult()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    var transactionGroup = new TransactionGroup("Test Group", "Description", "", user);

    var transactions = new List<Transaction>
    {
      new Transaction("Income", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 100.123456m, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user),
      new Transaction("Expense", "Description", TransactionTypeEnum.Expense,
        new Money { Amount = 50.987654m, Currency = CurrencyEnum.USD }, transactionGroup, DateTime.UtcNow, user)
    };

    UserRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<User> { user });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactions);

    var query = new GetTransactionSumQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(49.14m, result.Data!.Amount); // 100.123456 - 50.987654 = 49.135802, rounded to 49.14
    Assert.Equal(CurrencyEnum.USD, result.Data.Currency);
  }

  [Fact]
  public async Task Handle_DifferentUserBaseCurrency_ReturnsCorrectCurrency()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.EUR);
    var transactionGroup = new TransactionGroup("Test Group", "Description", "", user);

    var transactions = new List<Transaction>
    {
      new Transaction("Income EUR", "Description", TransactionTypeEnum.Income,
        new Money { Amount = 100, Currency = CurrencyEnum.EUR }, transactionGroup, DateTime.UtcNow, user),
      new Transaction("Expense EUR", "Description", TransactionTypeEnum.Expense,
        new Money { Amount = 25, Currency = CurrencyEnum.EUR }, transactionGroup, DateTime.UtcNow, user)
    };

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(transactions);

    var query = new GetTransactionSumQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(75, result.Data!.Amount); // 100 - 25
    Assert.Equal(CurrencyEnum.EUR, result.Data.Currency);
  }
}
