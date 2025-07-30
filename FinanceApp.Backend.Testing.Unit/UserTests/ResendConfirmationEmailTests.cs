using FinanceApp.Backend.Application.UserApi.UserCommands.ResendConfirmationEmail;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.UserTests;

public class ResendConfirmationEmailTests : TestBase
{
  private readonly Mock<ILogger<ResendConfirmationEmailCommandHandler>> _loggerMock;
  private readonly ResendConfirmationEmailCommandHandler _handler;

  public ResendConfirmationEmailTests()
  {
    _loggerMock = CreateLoggerMock<ResendConfirmationEmailCommandHandler>();

    _handler = new ResendConfirmationEmailCommandHandler(
      _loggerMock.Object,
      Mapper,
      UserRepositorySpecificMock.Object,
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

    var user = new Domain.Entities.User("testuser", email, "hashedpassword", CurrencyEnum.USD)
    {
      Id = Guid.NewGuid(),
      IsEmailConfirmed = false
    };

    UserRepositorySpecificMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    TokenServiceMock.Setup(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation))
                    .Returns(Task.FromResult(Result<string>.Success("confirmation_token")));

    SmtpEmailSenderMock.Setup(x => x.SendEmailConfirmationAsync(user, "confirmation_token"))
                       .Returns(Task.FromResult(Result<bool>.Success(true)));

    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    UserRepositorySpecificMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
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

    UserRepositorySpecificMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync((Domain.Entities.User?)null);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError.Code);
    UserRepositorySpecificMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()), Times.Never);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ResendConfirmationEmailHandler_EmailAlreadyConfirmed_ReturnsSuccessResult()
  {
    // arrange
    var email = "confirmed@example.com";
    var command = new ResendConfirmationEmailCommand(new() { Email = email }, CancellationToken.None);

    var user = new Domain.Entities.User("testuser", email, "hashedpassword", CurrencyEnum.USD)
    {
      Id = Guid.NewGuid(),
      IsEmailConfirmed = true
    };

    UserRepositorySpecificMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal("Email already confirmed.", result.Data.Message);
    UserRepositorySpecificMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()), Times.Never);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ResendConfirmationEmailHandler_TokenGenerationFails_ReturnsFailureResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ResendConfirmationEmailCommand(new() { Email = email }, CancellationToken.None);

    var user = new Domain.Entities.User("testuser", email, "hashedpassword", CurrencyEnum.USD)
    {
      Id = Guid.NewGuid(),
      IsEmailConfirmed = false
    };

    UserRepositorySpecificMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
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
    UserRepositorySpecificMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(It.IsAny<Domain.Entities.User>(), It.IsAny<string>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ResendConfirmationEmailHandler_EmailSendingFails_ReturnsFailureResult()
  {
    // arrange
    var email = "test@example.com";
    var command = new ResendConfirmationEmailCommand(new() { Email = email }, CancellationToken.None);

    var user = new Domain.Entities.User("testuser", email, "hashedpassword", CurrencyEnum.USD)
    {
      Id = Guid.NewGuid(),
      IsEmailConfirmed = false
    };

    UserRepositorySpecificMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
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
    UserRepositorySpecificMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
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

    var user = new Domain.Entities.User("testuser", email, "hashedpassword", CurrencyEnum.USD)
    {
      Id = Guid.NewGuid(),
      IsEmailConfirmed = false
    };

    UserRepositorySpecificMock.Setup(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()))
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
    UserRepositorySpecificMock.Verify(x => x.GetUserByEmailAsync(email, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(email, TokenType.EmailConfirmation), Times.Once);
    SmtpEmailSenderMock.Verify(x => x.SendEmailConfirmationAsync(user, "confirmation_token"), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
