using FinanceApp.Backend.Application.UserApi.UserCommands.DeleteUser;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.UserTests;

public class DeleteUserTests : TestBase
{
  private readonly Mock<ILogger<DeleteUserCommandHandler>> _loggerMock;
  private readonly DeleteUserCommandHandler _handler;

  public DeleteUserTests()
  {
    _loggerMock = CreateLoggerMock<DeleteUserCommandHandler>();

    _handler = new DeleteUserCommandHandler(
      _loggerMock.Object,
      UserRepositoryMock.Object,
      UnitOfWorkMock.Object,
      TransactionRepositoryMock.Object,
      TransactionGroupRepositoryMock.Object
    );
  }

  [Fact]
  public async Task DeleteUserHandler_ValidRequest_ReturnsSuccessResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var existingUser = new Domain.Entities.User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    var command = new DeleteUserCommand(userId, CancellationToken.None);

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    UserRepositoryMock.Verify(x => x.DeleteAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task DeleteUserHandler_UserNotFound_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var command = new DeleteUserCommand(userId, CancellationToken.None);

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Domain.Entities.User?)null);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    TransactionGroupRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    UserRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task DeleteUserHandler_TransactionRepositoryThrowsException_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var existingUser = new Domain.Entities.User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    var command = new DeleteUserCommand(userId, CancellationToken.None);

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    TransactionRepositoryMock
      .Setup(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("Database error"));

    // act & assert
    var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    Assert.Equal("Database error", exception.Message);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    UserRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task DeleteUserHandler_TransactionGroupRepositoryThrowsException_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var existingUser = new Domain.Entities.User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    var command = new DeleteUserCommand(userId, CancellationToken.None);

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    TransactionGroupRepositoryMock
      .Setup(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("Database error"));

    // act & assert
    var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    Assert.Equal("Database error", exception.Message);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    UserRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task DeleteUserHandler_UserRepositoryDeleteThrowsException_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var existingUser = new Domain.Entities.User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    var command = new DeleteUserCommand(userId, CancellationToken.None);

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    UserRepositoryMock
      .Setup(x => x.DeleteAsync(existingUser, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("Database error"));

    // act & assert
    var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    Assert.Equal("Database error", exception.Message);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    UserRepositoryMock.Verify(x => x.DeleteAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task DeleteUserHandler_UnitOfWorkThrowsException_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var existingUser = new Domain.Entities.User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    var command = new DeleteUserCommand(userId, CancellationToken.None);

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    UnitOfWorkMock
      .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("Database error"));

    // act & assert
    var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    Assert.Equal("Database error", exception.Message);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.DeleteAllByUserIdAsync(existingUser.Id, It.IsAny<CancellationToken>()), Times.Once);
    UserRepositoryMock.Verify(x => x.DeleteAsync(existingUser, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Theory]
  [InlineData(CurrencyEnum.USD)]
  [InlineData(CurrencyEnum.EUR)]
  [InlineData(CurrencyEnum.GBP)]
  public async Task DeleteUserHandler_DifferentBaseCurrencies_DeletesUserWithCorrectCurrency(CurrencyEnum baseCurrency)
  {
    // arrange
    var userId = Guid.NewGuid();
    var existingUser = new Domain.Entities.User("testuser", "test@example.com", "hashedpassword", baseCurrency);
    var command = new DeleteUserCommand(userId, CancellationToken.None);

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);

    UserRepositoryMock.Verify(x => x.DeleteAsync(It.Is<Domain.Entities.User>(u => u.BaseCurrency == baseCurrency), It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task DeleteUserHandler_EmptyGuid_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.Empty;
    var command = new DeleteUserCommand(userId, CancellationToken.None);

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync((Domain.Entities.User?)null);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
  }
}
