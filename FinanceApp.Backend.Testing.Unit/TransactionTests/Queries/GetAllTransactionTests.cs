using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetAllTransaction;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Queries;

public class GetAllTransactionTests : TestBase
{
  private readonly Mock<ILogger<GetAllTransactionQueryHandler>> _loggerMock;
  private readonly GetAllTransactionQueryHandler _handler;

  public GetAllTransactionTests()
  {
    _loggerMock = CreateLoggerMock<GetAllTransactionQueryHandler>();
    _handler = new GetAllTransactionQueryHandler(
      _loggerMock.Object,
      Mapper,
      TransactionRepositoryMock.Object,
      UserServiceMock.Object,
      ExchangeRateServiceMock.Object
    );
  }

  [Fact]
  public async Task GetAllTransactions_ValidUser_ReturnsTransactions()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);

    var transaction = new Transaction(
      "Test Transaction",
      "Description",
      TransactionTypeEnum.Income,
      new Money()
      {
        Amount = 100,
        Currency = CurrencyEnum.USD
      },
      100m,
      new TransactionGroup("Test Group", "Description", "", user),
      DateTimeOffset.UtcNow,
      user
    );

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result<User>.Success(user));
    TransactionRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Transaction> { transaction });


    var query = new GetAllTransactionQuery(CancellationToken.None, null);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetAllTransactions_UserNotFound_ReturnsFailure()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);

    var transaction = new Transaction(
      "Test Transaction",
      "Description",
      TransactionTypeEnum.Income,
      new Money()
      {
        Amount = 100,
        Currency = CurrencyEnum.USD
      },
      100m,
      new TransactionGroup("Test Group", "Description", "", user),
      DateTimeOffset.UtcNow,
      user
    );

    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure<User>(ApplicationError.UserNotFoundError()));
    var query = new GetAllTransactionQuery(CancellationToken.None, null);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
  }
}
