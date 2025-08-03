using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionById;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Queries;

public class GetTransactionByIdQueryTests : TestBase
{
  private readonly Mock<ILogger<GetTransactionByIdQueryHandler>> _loggerMock;
  private readonly Mock<IRepository<Transaction>> _transactionRepositoryMock;
  private readonly GetTransactionByIdQueryHandler _handler;

  public GetTransactionByIdQueryTests()
  {
    _loggerMock = CreateLoggerMock<GetTransactionByIdQueryHandler>();
    _transactionRepositoryMock = new Mock<IRepository<Transaction>>();
    _handler = new GetTransactionByIdQueryHandler(
        _loggerMock.Object,
        Mapper,
        _transactionRepositoryMock.Object
    );
  }

  [Fact]
  public async Task Handle_WithValidTransactionId_ShouldReturnSuccessResult()
  {
    // arrange
    var transactionId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    var user = new User("testuser", "test@example.com", "hash", CurrencyEnum.USD) { Id = userId };

    var transactionGroup = new TransactionGroup("Shopping", "Shopping expenses", null, user)
    {
      Id = Guid.NewGuid()
    };

    var transaction = new Transaction(
      "Amazon Purchase",
      "Online shopping",
      TransactionTypeEnum.Expense,
      new Money { Amount = 150.75m, Currency = CurrencyEnum.USD },
      transactionGroup,
      DateTimeOffset.UtcNow.AddDays(-1),
      user)
    {
      Id = transactionId
    };

    _transactionRepositoryMock
      .Setup(x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(transaction);

    var query = new GetTransactionByIdQuery(transactionId, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(transactionId, result.Data.Id);
    Assert.Equal("Amazon Purchase", result.Data.Name);
    Assert.Equal("Online shopping", result.Data.Description);
    Assert.Equal(150.75m, result.Data.Value.Amount);
    Assert.Equal(CurrencyEnum.USD, result.Data.Value.Currency);
    Assert.Equal(TransactionTypeEnum.Expense, result.Data.TransactionType);
    Assert.NotNull(result.Data.TransactionGroup);
    Assert.Equal("Shopping", result.Data.TransactionGroup.Name);

    _transactionRepositoryMock.Verify(
      x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithValidTransactionIdNoGroup_ShouldReturnSuccessResultWithoutGroup()
  {
    // arrange
    var transactionId = Guid.NewGuid();
    var userId = Guid.NewGuid();
    var user = new User("testuser", "test@example.com", "hash", CurrencyEnum.EUR) { Id = userId };

    var transaction = new Transaction(
      "Cash Payment",
      null, // No description
      TransactionTypeEnum.Income,
      new Money { Amount = 500.00m, Currency = CurrencyEnum.EUR },
      null, // No transaction group
      DateTimeOffset.UtcNow,
      user)
    {
      Id = transactionId
    };

    _transactionRepositoryMock
      .Setup(x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(transaction);

    var query = new GetTransactionByIdQuery(transactionId, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(transactionId, result.Data.Id);
    Assert.Equal("Cash Payment", result.Data.Name);
    Assert.Null(result.Data.Description);
    Assert.Equal(500.00m, result.Data.Value.Amount);
    Assert.Equal(CurrencyEnum.EUR, result.Data.Value.Currency);
    Assert.Equal(TransactionTypeEnum.Income, result.Data.TransactionType);
    Assert.Null(result.Data.TransactionGroup);

    _transactionRepositoryMock.Verify(
      x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithNonExistentTransactionId_ShouldReturnSuccessWithNullData()
  {
    // arrange
    var transactionId = Guid.NewGuid();

    _transactionRepositoryMock
      .Setup(x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Transaction?)null);

    var query = new GetTransactionByIdQuery(transactionId, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Null(result.Data);

    _transactionRepositoryMock.Verify(
      x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_WithEmptyGuid_ShouldCallRepositoryWithEmptyGuid()
  {
    // arrange
    var emptyGuid = Guid.Empty;

    _transactionRepositoryMock
      .Setup(x => x.GetByIdAsync(emptyGuid, true, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Transaction?)null);

    var query = new GetTransactionByIdQuery(emptyGuid, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Null(result.Data);

    _transactionRepositoryMock.Verify(
      x => x.GetByIdAsync(emptyGuid, true, It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldUseNoTrackingTrue()
  {
    // arrange
    var transactionId = Guid.NewGuid();
    var query = new GetTransactionByIdQuery(transactionId, CancellationToken.None);

    _transactionRepositoryMock
      .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Transaction?)null);

    // act
    await _handler.Handle(query, CancellationToken.None);

    // assert
    _transactionRepositoryMock.Verify(
      x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Handle_ShouldPassCancellationTokenToRepository()
  {
    // arrange
    var transactionId = Guid.NewGuid();
    var cancellationToken = new CancellationToken();
    var query = new GetTransactionByIdQuery(transactionId, cancellationToken);

    _transactionRepositoryMock
      .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Transaction?)null);

    // act
    await _handler.Handle(query, cancellationToken);

    // assert
    _transactionRepositoryMock.Verify(
      x => x.GetByIdAsync(transactionId, true, cancellationToken),
      Times.Once);
  }

  [Theory]
  [InlineData(TransactionTypeEnum.Expense)]
  [InlineData(TransactionTypeEnum.Income)]
  public async Task Handle_WithDifferentTransactionTypes_ShouldReturnCorrectType(TransactionTypeEnum transactionType)
  {
    // arrange
    var transactionId = Guid.NewGuid();
    var user = new User("testuser", "test@example.com", "hash", CurrencyEnum.USD) { Id = Guid.NewGuid() };

    var transaction = new Transaction(
      "Test Transaction",
      "Test Description",
      transactionType,
      new Money { Amount = 100m, Currency = CurrencyEnum.USD },
      null,
      DateTimeOffset.UtcNow,
      user)
    {
      Id = transactionId
    };

    _transactionRepositoryMock
      .Setup(x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(transaction);

    var query = new GetTransactionByIdQuery(transactionId, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(transactionType, result.Data.TransactionType);
  }

  [Theory]
  [InlineData(CurrencyEnum.USD)]
  [InlineData(CurrencyEnum.EUR)]
  [InlineData(CurrencyEnum.GBP)]
  public async Task Handle_WithDifferentCurrencies_ShouldReturnCorrectCurrency(CurrencyEnum currency)
  {
    // arrange
    var transactionId = Guid.NewGuid();
    var user = new User("testuser", "test@example.com", "hash", currency) { Id = Guid.NewGuid() };

    var transaction = new Transaction(
      "Test Transaction",
      "Test Description",
      TransactionTypeEnum.Expense,
      new Money { Amount = 100m, Currency = currency },
      null,
      DateTimeOffset.UtcNow,
      user)
    {
      Id = transactionId
    };

    _transactionRepositoryMock
      .Setup(x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(transaction);

    var query = new GetTransactionByIdQuery(transactionId, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(currency, result.Data.Value.Currency);
  }

  [Fact]
  public async Task Handle_ShouldLogTransactionRetrieval()
  {
    // arrange
    var transactionId = Guid.NewGuid();
    var query = new GetTransactionByIdQuery(transactionId, CancellationToken.None);

    _transactionRepositoryMock
      .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((Transaction?)null);

    // act
    await _handler.Handle(query, CancellationToken.None);

    // assert
    _loggerMock.Verify(
      x => x.Log(
        LogLevel.Information,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Transaction retrieved with ID:{transactionId}")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once);
  }
}
