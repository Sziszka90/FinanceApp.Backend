using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.UserApi.UserCommands.UpdateUser;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.UserTests;

public class UpdateUserTests : TestBase
{
  private readonly Mock<ILogger<UpdateUserCommandHandler>> _loggerMock;
  private readonly UpdateUserCommandHandler _handler;

  public UpdateUserTests()
  {
    _loggerMock = CreateLoggerMock<UpdateUserCommandHandler>();
    _handler = new UpdateUserCommandHandler(
      _loggerMock.Object,
      Mapper,
      UserRepositoryMock.Object,
      UnitOfWorkMock.Object,
      HttpContextAccessorMock.Object,
      BcryptServiceMock.Object
    );
  }

  [Fact]
  public async Task UpdateUserHandler_ValidRequest_UpdatesUserAndReturnsSuccess()
  {
    // arrange
    var userId = Guid.NewGuid();
    var newUserName = "updateduser";
    var newCurrency = CurrencyEnum.EUR;
    var newPassword = "NewPassword123!";
    var hashedPassword = "hashed_new_password";

    var updateUserDto = new UpdateUserRequest
    {
      Id = userId,
      UserName = newUserName,
      Password = newPassword,
      BaseCurrency = newCurrency
    };

    var command = new UpdateUserCommand(updateUserDto, CancellationToken.None);

    var user = new User(null, "testuser", "test@example.com", true, "oldhash", CurrencyEnum.USD)
    {
      Id = userId
    };

    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    BcryptServiceMock.Setup(x => x.Hash(newPassword)).Returns(hashedPassword);

    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(newUserName, user.UserName);
    Assert.Equal(hashedPassword, user.PasswordHash);
    Assert.Equal(newCurrency, user.BaseCurrency);
    BcryptServiceMock.Verify(x => x.Hash(newPassword), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task UpdateUserHandler_UserNotFound_ReturnsFailure()
  {
    // arrange
    var userId = Guid.NewGuid();
    var updateUserDto = new UpdateUserRequest
    {
      Id = userId,
      UserName = "updateduser",
      Password = "NewPassword123!",
      BaseCurrency = CurrencyEnum.EUR
    };
    var command = new UpdateUserCommand(updateUserDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("ENTITY_NOT_FOUND", result.ApplicationError.Code);
    BcryptServiceMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task UpdateUserHandler_UnitOfWorkThrowsException_Throws()
  {
    // arrange
    var userId = Guid.NewGuid();
    var newUserName = "updateduser";
    var newCurrency = CurrencyEnum.EUR;
    var newPassword = "NewPassword123!";
    var hashedPassword = "hashed_new_password";

    var updateUserDto = new UpdateUserRequest
    {
      Id = userId,
      UserName = newUserName,
      Password = newPassword,
      BaseCurrency = newCurrency
    };
    var command = new UpdateUserCommand(updateUserDto, CancellationToken.None);

    var user = new User(null, "testuser", "test@example.com", true, "oldhash", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    BcryptServiceMock.Setup(x => x.Hash(newPassword)).Returns(hashedPassword);

    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("Database error"));

    // act & assert
    var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    Assert.Equal("Database error", exception.Message);
    BcryptServiceMock.Verify(x => x.Hash(newPassword), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
