using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.UserApi.UserCommands.ConfirmUserEmail;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.UserTests.Commands;

public class ConfirmUserEmailTests : TestBase
{
  private readonly Mock<ILogger<ConfirmUserEmailCommandHandler>> _loggerMock;
  private readonly ConfirmUserEmailCommandHandler _handler;

  public ConfirmUserEmailTests()
  {
    _loggerMock = CreateLoggerMock<ConfirmUserEmailCommandHandler>();

    _handler = new ConfirmUserEmailCommandHandler(
      _loggerMock.Object,
      UserRepositoryMock.Object,
      UnitOfWorkMock.Object,
      TokenServiceMock.Object
    );
  }

  [Fact]
  public async Task ConfirmUserEmailHandler_ValidRequest_ReturnsSuccessResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var token = "valid_email_confirmation_token";
    var command = new ConfirmUserEmailCommand(userId, token, CancellationToken.None);

    var user = new Domain.Entities.User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);

    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(user);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.True(user.IsEmailConfirmed);
    Assert.Null(user.EmailConfirmationToken);
    Assert.Null(user.EmailConfirmationTokenExpiration);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.ValidateTokenAsync(token, TokenType.EmailConfirmation, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task ConfirmUserEmailHandler_UserNotFound_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var token = "valid_token_123";
    var command = new ConfirmUserEmailCommand(userId, token, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ConfirmUserEmailHandler_TokenExpired_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var token = "expired_token_123";
    var command = new ConfirmUserEmailCommand(userId, token, CancellationToken.None);

    var existingUser = new User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    existingUser.SetEmailConfirmationToken(token, DateTimeOffset.UtcNow.AddHours(-1));

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("TOKEN_EXPIRED", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ConfirmUserEmailHandler_InvalidToken_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var token = "invalid_token_123";
    var command = new ConfirmUserEmailCommand(userId, token, CancellationToken.None);

    var existingUser = new User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    existingUser.SetEmailConfirmationToken("different_token", DateTimeOffset.UtcNow.AddHours(1));

    var tokenError = ApplicationError.InvalidTokenError();

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    TokenServiceMock
      .Setup(x => x.ValidateTokenAsync(token, TokenType.EmailConfirmation, It.IsAny<CancellationToken>()))
      .Returns(Task.FromResult(Result.Failure<bool>(tokenError)));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("INVALID_TOKEN", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.ValidateTokenAsync(token, TokenType.EmailConfirmation, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ConfirmUserEmailHandler_TokenServiceThrowsException_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var token = "valid_token_123";
    var command = new ConfirmUserEmailCommand(userId, token, CancellationToken.None);

    var existingUser = new User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    existingUser.SetEmailConfirmationToken(token, DateTimeOffset.UtcNow.AddHours(1));

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    TokenServiceMock
      .Setup(x => x.ValidateTokenAsync(token, TokenType.EmailConfirmation, It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("Token service error"));

    // act & assert
    var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
    Assert.Equal("Token service error", exception.Message);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.ValidateTokenAsync(token, TokenType.EmailConfirmation, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task ConfirmUserEmailHandler_UnitOfWorkThrowsException_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.NewGuid();
    var token = "valid_token_123";
    var command = new ConfirmUserEmailCommand(userId, token, CancellationToken.None);

    var existingUser = new User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    existingUser.SetEmailConfirmationToken(token, DateTimeOffset.UtcNow.AddHours(1));

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
    TokenServiceMock.Verify(x => x.ValidateTokenAsync(token, TokenType.EmailConfirmation, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Theory]
  [InlineData(CurrencyEnum.USD)]
  [InlineData(CurrencyEnum.EUR)]
  [InlineData(CurrencyEnum.GBP)]
  public async Task ConfirmUserEmailHandler_DifferentBaseCurrencies_ConfirmsUserWithCorrectCurrency(CurrencyEnum baseCurrency)
  {
    // arrange
    var userId = Guid.NewGuid();
    var token = "valid_token_123";
    var command = new ConfirmUserEmailCommand(userId, token, CancellationToken.None);

    var existingUser = new User("testuser", "test@example.com", "hashedpassword", baseCurrency);
    existingUser.SetEmailConfirmationToken(token, DateTimeOffset.UtcNow.AddHours(1));

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.True(existingUser.IsEmailConfirmed);
    Assert.Equal(baseCurrency, existingUser.BaseCurrency);

    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task ConfirmUserEmailHandler_EmptyGuid_ReturnsFailureResult()
  {
    // arrange
    var userId = Guid.Empty;
    var token = "valid_token_123";
    var command = new ConfirmUserEmailCommand(userId, token, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError.Code);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public async Task ConfirmUserEmailHandler_InvalidTokenFormat_HandledByValidation(string? invalidToken)
  {
    // Note: This test demonstrates that invalid token formats would be caught by validation
    // before reaching the handler, but we test the handler's robustness anyway

    // arrange
    var userId = Guid.NewGuid();
    var command = new ConfirmUserEmailCommand(userId, invalidToken!, CancellationToken.None);

    var existingUser = new User("testuser", "test@example.com", "hashedpassword", CurrencyEnum.USD);
    existingUser.SetEmailConfirmationToken("valid_token", DateTimeOffset.UtcNow.AddHours(1));

    UserRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .ReturnsAsync(existingUser);

    var tokenError = ApplicationError.InvalidTokenError();
    TokenServiceMock
      .Setup(x => x.ValidateTokenAsync(invalidToken!, TokenType.EmailConfirmation, It.IsAny<CancellationToken>()))
      .Returns(Task.FromResult(Result.Failure<bool>(tokenError)));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);

    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.ValidateTokenAsync(invalidToken!, TokenType.EmailConfirmation, It.IsAny<CancellationToken>()), Times.Once);
  }
}
