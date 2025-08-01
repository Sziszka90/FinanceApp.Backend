using FinanceApp.Backend.Application.UserApi.UserCommands.ForgotPassword;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Testing.Unit.UserTests.Commands;

public class ForgotPasswordTests : TestBase
{
  private readonly Mock<ILogger<ForgotPasswordCommandHandler>> _loggerMock;
  private readonly ForgotPasswordCommandHandler _handler;

  public ForgotPasswordTests()
  {
    _loggerMock = CreateLoggerMock<ForgotPasswordCommandHandler>();

    _handler = new ForgotPasswordCommandHandler(
      _loggerMock.Object,
      SmtpEmailSenderMock.Object,
      UserRepositoryMock.Object,
      UnitOfWorkMock.Object,
      TokenServiceMock.Object
    );
  }

  [Fact]
  public async Task ForgotPasswordHandler_ValidRequest_ReturnsSuccessResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ForgotPasswordCommand(new() { Email = email }, CancellationToken.None);

    var user = new User("testuser", email, "hashedpassword", CurrencyEnum.USD)
    {
      Id = Guid.NewGuid(),
      IsEmailConfirmed = true
    };

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    TokenServiceMock.Setup(x => x.GenerateTokenAsync(email, TokenType.PasswordReset))
                    .Returns(Task.FromResult(Result<string>.Success("reset_token")));

    SmtpEmailSenderMock.Setup(x => x.SendForgotPasswordAsync(email, "reset_token"))
                       .Returns(Task.FromResult(Result<bool>.Success(true)));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.PasswordReset), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendForgotPasswordAsync(email, "reset_token"), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task ForgotPasswordHandler_UserNotFound_ReturnsFailureResult()
  {
    // arrange
    var email = "notfound@example.com";
    var command = new ForgotPasswordCommand(new() { Email = email }, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError.Code);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()), Times.Never);
    SmtpEmailSenderMock.Verify(x => x.SendForgotPasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ForgotPasswordHandler_EmailNotConfirmed_ReturnsFailureResult()
  {
    // arrange
    var email = "unconfirmed@example.com";
    var command = new ForgotPasswordCommand(new() { Email = email }, CancellationToken.None);

    var user = new User("testuser", email, "hashedpassword", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USEREMAIL_CONFIRMATION_ERROR", result.ApplicationError.Code);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()), Times.Never);
    SmtpEmailSenderMock.Verify(x => x.SendForgotPasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ForgotPasswordHandler_TokenGenerationFails_ReturnsFailureResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ForgotPasswordCommand(new() { Email = email }, CancellationToken.None);

    var user = new User(new Guid(), "testuser", email, true, "hashedpassword", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    var tokenError = ApplicationError.TokenGenerationError();
    TokenServiceMock.Setup(x => x.GenerateTokenAsync(email, TokenType.PasswordReset))
                    .Returns(Task.FromResult(Result.Failure<string>(tokenError)));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("TOKEN_GENERATION_ERROR", result.ApplicationError.Code);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.PasswordReset), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendForgotPasswordAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ForgotPasswordHandler_EmailSendingFails_ReturnsFailureResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ForgotPasswordCommand(new() { Email = email }, CancellationToken.None);

    var user = new User("testuser", email, "hashedpassword", CurrencyEnum.USD)
    {
      Id = Guid.NewGuid(),
      IsEmailConfirmed = true
    };

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    TokenServiceMock.Setup(x => x.GenerateTokenAsync(email, TokenType.PasswordReset))
                    .Returns(Task.FromResult(Result<string>.Success("reset_token")));

    SmtpEmailSenderMock.Setup(x => x.SendForgotPasswordAsync(email, "reset_token"))
                       .Returns(Task.FromResult(Result.Failure<bool>(ApplicationError.ExternalCallError("Email send failed"))));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("EXT_CALL_ERROR", result.ApplicationError.Code);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.PasswordReset), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendForgotPasswordAsync(email, "reset_token"), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task ForgotPasswordHandler_UnitOfWorkThrowsException_ReturnsFailureResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ForgotPasswordCommand(new() { Email = email }, CancellationToken.None);

    var user = new User(new Guid(), "testuser", email, true, "hashedpassword", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    TokenServiceMock.Setup(x => x.GenerateTokenAsync(email, TokenType.PasswordReset))
                    .Returns(Task.FromResult(Result<string>.Success("reset_token")));

    SmtpEmailSenderMock.Setup(x => x.SendForgotPasswordAsync(email, "reset_token"))
                       .Returns(Task.FromResult(Result<bool>.Success(true)));

    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new Exception("Database error"));

    // act & assert
    var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    Assert.Equal("Database error", exception.Message);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.PasswordReset), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendForgotPasswordAsync(email, "reset_token"), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
