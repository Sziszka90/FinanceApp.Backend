using FinanceApp.Backend.Application.AuthApi.AuthCommands.Login;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using RTools_NTS.Util;

namespace FinanceApp.Backend.Testing.Unit.AuthTests;

public class LoginTests : TestBase
{
  protected readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
  protected readonly LoginCommandHandler _handler;

  public LoginTests()
  {
    _loggerMock = CreateLoggerMock<LoginCommandHandler>();

    _handler = new LoginCommandHandler(
      _loggerMock.Object,
      UserRepositorySpecificMock.Object,
      TokenServiceMock.Object
    );
  }

  [Fact]
  public async Task LoginHandler_ValidRequest_ReturnsSuccess()
  {
    // arrange
    var email = "test@example.com";
    var password = "Password123!";
    var hashedPassword = "default_hashed_password";
    var user = new User(null, "testuser", email, true, hashedPassword, CurrencyEnum.USD);
    var token = "default_token";

    var loginDto = new LoginRequestDto
    {
      Email = email,
      Password = password
    };
    var command = new LoginCommand(loginDto, CancellationToken.None);

    UserRepositorySpecificMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
        .ReturnsAsync(user);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(token, result.Data.Token);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(user.Email, It.IsAny<TokenType>()), Times.Once);
  }

  [Fact]
  public async Task LoginHandler_UserNotFound_ReturnsFailure()
  {
    // arrange
    var email = "notfound@example.com";
    var password = "Password123!";
    var loginDto = new LoginRequestDto
    {
      Email = email,
      Password = password
    };
    var command = new LoginCommand(loginDto, CancellationToken.None);

    UserRepositorySpecificMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
        .ReturnsAsync((User?)null);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("ENTITY_NOT_FOUND", result.ApplicationError.Code);
    BcryptServiceMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  public async Task LoginHandler_InvalidPassword_ReturnsFailure()
  {
    // arrange
    var email = "test@example.com";
    var password = "WrongPassword!";
    var hashedPassword = "hashed_password";
    var user = new User(null, "testuser", email, true, hashedPassword, CurrencyEnum.USD);

    var loginDto = new LoginRequestDto
    {
      Email = email,
      Password = password
    };
    var command = new LoginCommand(loginDto, CancellationToken.None);

    UserRepositorySpecificMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
        .ReturnsAsync(user);
    BcryptServiceMock.Setup(x => x.Verify(password, hashedPassword)).Returns(false);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("INVALID_CREDENTIALS", result.ApplicationError.Code);
    BcryptServiceMock.Verify(x => x.Verify(password, hashedPassword), Times.Once);
  }
}
