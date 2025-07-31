using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.UserApi.UserCommands.UpdatePassword;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.UserTests;

public class UpdatePasswordTests : TestBase
{
  private readonly Mock<ILogger<UpdatePasswordCommandHandler>> _loggerMock;
  private readonly UpdatePasswordCommandHandler _handler;

  public UpdatePasswordTests()
  {
    _loggerMock = CreateLoggerMock<UpdatePasswordCommandHandler>();

    _handler = new UpdatePasswordCommandHandler(
      _loggerMock.Object,
      UserRepositoryMock.Object,
      UnitOfWorkMock.Object,
      TokenServiceMock.Object,
      BcryptServiceMock.Object
    );
  }

  [Fact]
  public async Task UpdatePasswordHandler_ValidRequest_UpdatesPasswordAndReturnsSuccess()
  {
    // arrange
    var newPassword = "NewPassword123!";
    var token = "valid_token";

    var updatePasswordDto = new UpdatePasswordRequest
    {
      Password = newPassword,
      Token = token
    };

    var command = new UpdatePasswordCommand(updatePasswordDto, CancellationToken.None);

    var user = new User(null, "testuser", "test@example.com", true, "oldhash", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    TokenServiceMock
      .Setup(x => x.ValidateTokenAsync(token, It.IsAny<TokenType>()))
      .ReturnsAsync(Result.Success(true));

    TokenServiceMock
      .Setup(x => x.GetEmailFromTokenAsync(token))
      .Returns(user.Email);

    BcryptServiceMock.Setup(x => x.Hash(newPassword)).Returns("hashed_new_password");

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal("hashed_new_password", user.PasswordHash);
    BcryptServiceMock.Verify(x => x.Hash(newPassword), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task UpdatePasswordHandler_UserNotFound_ReturnsFailure()
  {
    // arrange
    var userId = Guid.NewGuid();
    var newPassword = "NewPassword123!";
    var token = "valid_token";

    var command = new UpdatePasswordCommand(new UpdatePasswordRequest
    {
      Password = newPassword,
      Token = token
    }, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError.Code);
    BcryptServiceMock.Verify(x => x.Hash(It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task UpdatePasswordHandler_UnitOfWorkThrowsException_Throws()
  {
    // arrange
    var userId = Guid.NewGuid();
    var newPassword = "NewPassword123!";
    var token = "valid_token";

    var command = new UpdatePasswordCommand(new UpdatePasswordRequest
    {
      Password = newPassword,
      Token = token
    }, CancellationToken.None);

    var user = new User(userId, "testuser", "test@example.com", true, "oldhash", CurrencyEnum.USD);

    BcryptServiceMock.Setup(x => x.Hash(newPassword)).Returns("hashed_new_password");

    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new Exception("Database error"));

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(user);

    // act & assert
    var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    Assert.Equal("Database error", exception.Message);
    BcryptServiceMock.Verify(x => x.Hash(newPassword), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
