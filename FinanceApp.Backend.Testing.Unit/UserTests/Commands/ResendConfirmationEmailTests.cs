using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.UserApi.UserCommands.ResendConfirmationEmail;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.UserTests.Commands;

public class ResendConfirmationEmailTests : TestBase
{
  private readonly Mock<ILogger<ResendConfirmationEmailCommandHandler>> _loggerMock;
  private readonly ResendConfirmationEmailCommandHandler _handler;

  public ResendConfirmationEmailTests()
  {
    _loggerMock = CreateLoggerMock<ResendConfirmationEmailCommandHandler>();

    _handler = new ResendConfirmationEmailCommandHandler(
      _loggerMock.Object,
      UserRepositoryMock.Object,
      UnitOfWorkMock.Object,
      SmtpEmailSenderMock.Object,
      TokenServiceMock.Object
    );
  }

  [Fact]
  public async Task ResendConfirmationEmailHandler_ValidRequest_ReturnsSuccessResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ResendConfirmationEmailCommand(new() { Email = email }, CancellationToken.None);

    var user = new User("testuser", email, "hashedpassword", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    TokenServiceMock.Setup(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation))
                    .Returns(Task.FromResult(Result<string>.Success("confirmation_token")));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(user, "confirmation_token"), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task ResendConfirmationEmailHandler_UserNotFound_ReturnsFailureResult()
  {
    // arrange
    var email = "notfound@example.com";
    var command = new ResendConfirmationEmailCommand(new() { Email = email }, CancellationToken.None);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((User?)null);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError.Code);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()), Times.Never);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ResendConfirmationEmailHandler_EmailAlreadyConfirmed_ReturnsSuccessResult()
  {
    // arrange
    var email = "confirmed@example.com";
    var command = new ResendConfirmationEmailCommand(new() { Email = email }, CancellationToken.None);

    var user = new User(new Guid(), "testuser", email, true, "hashedpassword", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal("Email already confirmed.", result.Data.Message);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()), Times.Never);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ResendConfirmationEmailHandler_TokenGenerationFails_ReturnsFailureResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ResendConfirmationEmailCommand(new() { Email = email }, CancellationToken.None);

    var user = new User("testuser", email, "hashedpassword", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    var tokenError = ApplicationError.TokenGenerationError();
    TokenServiceMock.Setup(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation))
                    .Returns(Task.FromResult(Result.Failure<string>(tokenError)));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("TOKEN_GENERATION_ERROR", result.ApplicationError.Code);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ResendConfirmationEmailHandler_EmailSendingFails_ReturnsFailureResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ResendConfirmationEmailCommand(new() { Email = email }, CancellationToken.None);

    var user = new User("testuser", email, "hashedpassword", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    TokenServiceMock.Setup(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation))
                    .Returns(Task.FromResult(Result<string>.Success("confirmation_token")));

    SmtpEmailSenderMock.Setup(x => x.SendEmailConfirmationAsync(user, "confirmation_token"))
                       .Returns(Task.FromResult(Result.Failure<bool>(ApplicationError.ExternalCallError("Email send failed"))));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("EXT_CALL_ERROR", result.ApplicationError.Code);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(user, "confirmation_token"), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task ResendConfirmationEmailHandler_UnitOfWorkThrowsException_ReturnsFailureResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ResendConfirmationEmailCommand(new() { Email = email }, CancellationToken.None);

    var user = new User("testuser", email, "hashedpassword", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    TokenServiceMock.Setup(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation))
                    .Returns(Task.FromResult(Result<string>.Success("confirmation_token")));

    SmtpEmailSenderMock.Setup(x => x.SendEmailConfirmationAsync(user, "confirmation_token"))
                       .Returns(Task.FromResult(Result<bool>.Success(true)));

    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new Exception("Database error"));

    // act & assert
    var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    Assert.Equal("Database error", exception.Message);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(user, "confirmation_token"), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
