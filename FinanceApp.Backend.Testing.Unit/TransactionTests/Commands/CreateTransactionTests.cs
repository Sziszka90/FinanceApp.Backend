using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.CreateTransaction;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class CreateTransactionTests : TestBase
{
  private readonly Mock<ILogger<CreateTransactionCommandHandler>> _loggerMock;
  private readonly CreateTransactionCommandHandler _handler;

  public CreateTransactionTests()
  {
    _loggerMock = CreateLoggerMock<CreateTransactionCommandHandler>();
    _handler = new CreateTransactionCommandHandler(
        _loggerMock.Object,
        Mapper,
        TransactionRepositoryMock.Object,
        TransactionGroupRepositoryMock.Object,
        UnitOfWorkMock.Object,
        UserServiceMock.Object,
        ExchangeRateServiceMock.Object
    );
  }

  [Fact]
  public async Task CreateTransaction_ValidRequest_CreatesTransaction()
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
      40.0m,
      new TransactionGroup("Test Group", "Description", "", user),
      DateTime.UtcNow,
      user
    );

    ExchangeRateServiceMock.Setup(x => x.ConvertAmountAsync(
      It.IsAny<decimal>(),
      It.IsAny<DateTimeOffset>(),
      It.IsAny<string>(),
      It.IsAny<string>(),
      It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(1.0m));
        
    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email, false, It.IsAny<CancellationToken>())).ReturnsAsync(user);
    TransactionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
    var createDto = new CreateTransactionDto();
    var command = new CreateTransactionCommand(createDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CreateTransaction_UserNotFound_ReturnsFailure()
  {
    // arrange
    var userEmail = "notfound@example.com";
    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Failure<User>(ApplicationError.UserNotFoundError(userEmail)));
    var createDto = new CreateTransactionDto();
    var command = new CreateTransactionCommand(createDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
