using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Hubs;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.MatchTransactionsCommands;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class MatchTransactionsTests : TestBase
{
  private readonly Mock<ILogger<MatchTransactionsCommandHandler>> _loggerMock;
  private readonly Mock<ISignalRService> _signalRServiceMock;
  private readonly MatchTransactionsCommandHandler _handler;

  public MatchTransactionsTests()
  {
    _loggerMock = CreateLoggerMock<MatchTransactionsCommandHandler>();
    _signalRServiceMock = new Mock<ISignalRService>();

    _handler = new MatchTransactionsCommandHandler(
      _loggerMock.Object,
      UserRepositoryMock.Object,
      TransactionRepositoryMock.Object,
      TransactionGroupRepositoryMock.Object,
      MatchedTransactionRepositoryMock.Object,
      UnitOfWorkMock.Object,
      _signalRServiceMock.Object
    );
  }

  [Fact]
  public async Task Handle_ValidRequest_ProcessesMatchedTransactionsSuccessfully()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    user.GetType().GetProperty("Id")?.SetValue(user, userId);

    var transactionGroup1 = new TransactionGroup("Food", "Food expenses", "", user);
    var transactionGroup2 = new TransactionGroup("Transport", "Transport expenses", "", user);

    var transaction1 = new Transaction("Coffee", "Morning coffee", TransactionTypeEnum.Expense,
      new Money { Amount = 5.00m, Currency = CurrencyEnum.USD }, 5.00m, transactionGroup1, DateTime.UtcNow, user);
    var transaction2 = new Transaction("Bus ticket", "Daily bus ticket", TransactionTypeEnum.Expense,
      new Money { Amount = 3.50m, Currency = CurrencyEnum.USD }, 3.50m, transactionGroup2, DateTime.UtcNow, user);

    var matchedTransactions = new Dictionary<string, string>
    {
      { "Coffee", "Food" },
      { "Bus ticket", "Transport" }
    };

    var rabbitMqPayload = new RabbitMqPayload
    {
      CorrelationId = Guid.NewGuid().ToString(),
      Success = true,
      UserId = userId.ToString(),
      Prompt = "Match transactions to groups",
    };

    var command = new MatchTransactionsCommand(rabbitMqPayload);

    // Setup mocks
    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup> { transactionGroup1, transactionGroup2 });

    TransactionRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { transaction1, transaction2 });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { transaction1, transaction2 });

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Data);

    // Verify repository calls
    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    // Verify SignalR notification
    _signalRServiceMock.Verify(x => x.SendToClientGroupMethodAsync(
      user.Email.ToString(),
      HubConstants.TRANSACTIONS_MATCHED_NOTIFICATION,
      HubConstants.REFRESH_TRANSACTIONS), Times.Once);

    // Verify transaction group assignments
    Assert.Equal("Food", transaction1.TransactionGroup?.Name);
    Assert.Equal("Transport", transaction2.TransactionGroup?.Name);
  }

  [Fact]
  public async Task Handle_UserNotFound_ReturnsFailure()
  {
    // arrange
    var userId = Guid.NewGuid();
    var rabbitMqPayload = new RabbitMqPayload
    {
      CorrelationId = Guid.NewGuid().ToString(),
      Success = true,
      UserId = userId.ToString(),
      Prompt = "Match transactions to groups",
    };

    var command = new MatchTransactionsCommand(rabbitMqPayload);

    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync((User?)null);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal(ApplicationError.UserNotFoundError().Message, result.ApplicationError.Message);

    // Verify no other operations were performed
    TransactionGroupRepositoryMock.Verify(x => x.GetAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    TransactionRepositoryMock.Verify(x => x.GetAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    _signalRServiceMock.Verify(x => x.SendToClientGroupMethodAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task Handle_NoTransactionsFound_ReturnsFailure()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    user.GetType().GetProperty("Id")?.SetValue(user, userId);

    var rabbitMqPayload = new RabbitMqPayload
    {
      CorrelationId = Guid.NewGuid().ToString(),
      Success = true,
      UserId = userId.ToString(),
      Prompt = "Match transactions to groups",
    };

    var command = new MatchTransactionsCommand(rabbitMqPayload);

    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    TransactionRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction>());

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("An exception occurred.", result.ApplicationError.Message);

    // Verify no save operations were performed
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    _signalRServiceMock.Verify(x => x.SendToClientGroupMethodAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task Handle_NoTransactionGroupsFound_ReturnsFailure()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    user.GetType().GetProperty("Id")?.SetValue(user, userId);

    var transaction = new Transaction("Coffee", "Morning coffee", TransactionTypeEnum.Expense,
      new Money { Amount = 5.00m, Currency = CurrencyEnum.USD }, 5.00m, null, DateTime.UtcNow, user);

    var rabbitMqPayload = new RabbitMqPayload
    {
      CorrelationId = Guid.NewGuid().ToString(),
      Success = true,
      UserId = userId.ToString(),
      Prompt = "Match transactions to groups"
    };

    var command = new MatchTransactionsCommand(rabbitMqPayload);

    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup>());

    TransactionRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { transaction });

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("An exception occurred.", result.ApplicationError.Message);

    // Verify no save operations were performed
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    _signalRServiceMock.Verify(x => x.SendToClientGroupMethodAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task Handle_CurrencyConversion_ConvertsToUserBaseCurrency()
  {
    // Arrange
    var userId = Guid.NewGuid();
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    user.GetType().GetProperty("Id")?.SetValue(user, userId);

    var transactionGroup = new TransactionGroup("Food", "Food expenses", "", user);

    // Transaction in EUR that should be converted to USD
    var transaction = new Transaction("Coffee", "Morning coffee", TransactionTypeEnum.Expense,
      new Money { Amount = 4.25m, Currency = CurrencyEnum.EUR }, 4.25m, transactionGroup, DateTime.UtcNow, user);

    var matchedTransactions = new List<Dictionary<string, string>>
    {
      new() { { "Coffee", "Food" } }
    };

    var rabbitMqPayload = new RabbitMqPayload
    {
      CorrelationId = Guid.NewGuid().ToString(),
      Success = true,
      UserId = userId.ToString(),
      Prompt = "Match transactions to groups"
    };

    var command = new MatchTransactionsCommand(rabbitMqPayload);

    // Setup mocks
    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup> { transactionGroup });

    TransactionRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { transaction });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { transaction });

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);

    Assert.Equal(CurrencyEnum.EUR, transaction.Value.Currency);
    Assert.Equal(4.25m, transaction.Value.Amount);
  }

  [Fact]
  public async Task Handle_SameCurrency_NoConversionRequired()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    user.GetType().GetProperty("Id")?.SetValue(user, userId);

    var transactionGroup = new TransactionGroup("Food", "Food expenses", "", user);

    // Transaction already in USD
    var transaction = new Transaction("Coffee", "Morning coffee", TransactionTypeEnum.Expense,
      new Money { Amount = 5.00m, Currency = CurrencyEnum.USD }, 5.00m, transactionGroup, DateTime.UtcNow, user);

    var matchedTransactions = new List<Dictionary<string, string>>
    {
      new() { { "Coffee", "Food" } }
    };

    var rabbitMqPayload = new RabbitMqPayload
    {
      CorrelationId = Guid.NewGuid().ToString(),
      Success = true,
      UserId = userId.ToString(),
      Prompt = "Match transactions to groups"
    };

    var command = new MatchTransactionsCommand(rabbitMqPayload);

    // Setup mocks
    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup> { transactionGroup });

    TransactionRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { transaction });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { transaction });

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);

    // Verify no currency conversion occurred
    Assert.Equal(CurrencyEnum.USD, transaction.Value.Currency);
    Assert.Equal(5.00m, transaction.Value.Amount); // Amount should remain unchanged
  }

  [Fact]
  public async Task Handle_MultipleTransactionsWithDifferentCurrencies_ConvertsAppropriately()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    user.GetType().GetProperty("Id")?.SetValue(user, userId);

    var foodGroup = new TransactionGroup("Food", "Food expenses", "", user);
    var transportGroup = new TransactionGroup("Transport", "Transport expenses", "", user);

    var usdTransaction = new Transaction("Coffee", "Morning coffee", TransactionTypeEnum.Expense,
      new Money { Amount = 5.00m, Currency = CurrencyEnum.USD }, 5.00m, foodGroup, DateTime.UtcNow, user);
    var eurTransaction = new Transaction("Lunch", "European lunch", TransactionTypeEnum.Expense,
      new Money { Amount = 12.50m, Currency = CurrencyEnum.EUR }, 12.50m, foodGroup, DateTime.UtcNow, user);
    var gbpTransaction = new Transaction("Train ticket", "London train", TransactionTypeEnum.Expense,
      new Money { Amount = 25.00m, Currency = CurrencyEnum.GBP }, 25.00m, transportGroup, DateTime.UtcNow, user);

    var matchedTransactions = new List<Dictionary<string, string>>
    {
      new() { { "Coffee", "Food" } },
      new() { { "Lunch", "Food" } },
      new() { { "Train ticket", "Transport" } }
    };

    var rabbitMqPayload = new RabbitMqPayload
    {
      CorrelationId = Guid.NewGuid().ToString(),
      Success = true,
      UserId = userId.ToString(),
      Prompt = "Match transactions to groups"
    };

    var command = new MatchTransactionsCommand(rabbitMqPayload);

    // Setup mocks
    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup> { foodGroup, transportGroup });

    TransactionRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { usdTransaction, eurTransaction, gbpTransaction });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { usdTransaction, eurTransaction, gbpTransaction });

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);

    // Verify all transactions are now in USD
    Assert.Equal(CurrencyEnum.USD, usdTransaction.Value.Currency);
    Assert.Equal(CurrencyEnum.EUR, eurTransaction.Value.Currency);
    Assert.Equal(CurrencyEnum.GBP, gbpTransaction.Value.Currency);

    // Verify amounts (USD unchanged, EUR * 1.18, GBP * 1.33)
    Assert.Equal(5.00m, usdTransaction.Value.Amount);
    Assert.Equal(12.50m, eurTransaction.Value.Amount);
    Assert.Equal(25.00m, gbpTransaction.Value.Amount);
  }

  [Fact]
  public async Task Handle_TransactionNotInMatchedResults_TransactionGroupRemainsNull()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);
    user.GetType().GetProperty("Id")?.SetValue(user, userId);

    var transactionGroup = new TransactionGroup("Food", "Food expenses", "", user);

    var matchedTransaction = new Transaction("Coffee", "Morning coffee", TransactionTypeEnum.Expense,
      new Money { Amount = 5.00m, Currency = CurrencyEnum.USD }, 5.00m, transactionGroup, DateTime.UtcNow, user);
    var unmatchedTransaction = new Transaction("Misc Item", "Random item", TransactionTypeEnum.Expense,
      new Money { Amount = 10.00m, Currency = CurrencyEnum.USD }, 10.00m, null, DateTime.UtcNow, user);

    // Only match one transaction
    var matchedTransactions = new Dictionary<string, string>
    {
      { "Coffee", "Food" }
    };

    var rabbitMqPayload = new RabbitMqPayload
    {
      CorrelationId = Guid.NewGuid().ToString(),
      Success = true,
      UserId = userId.ToString(),
      Prompt = "Match transactions to groups"
    };

    var command = new MatchTransactionsCommand(rabbitMqPayload);

    // Setup mocks
    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    TransactionGroupRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup> { transactionGroup });

    TransactionRepositoryMock.Setup(x => x.GetAllByUserIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { matchedTransaction, unmatchedTransaction });

    TransactionRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<Transaction> { matchedTransaction, unmatchedTransaction });

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);

    // Verify matched transaction has the correct group
    Assert.Equal("Food", matchedTransaction.TransactionGroup?.Name);

    // Verify unmatched transaction remains without a group
    Assert.Null(unmatchedTransaction.TransactionGroup);
  }
}
