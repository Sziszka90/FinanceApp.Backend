using FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;
using FinanceApp.Backend.Application.Dtos.TokenDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.AuthTests;

public class ValidateTokenCommandHandlerTests : TestBase
{
  private readonly Mock<ILogger<ValidateTokenCommandHandler>> _loggerMock = new();
  private readonly ValidateTokenCommandHandler _handler;

  public ValidateTokenCommandHandlerTests()
  {
    _handler = new ValidateTokenCommandHandler(_loggerMock.Object, TokenServiceMock.Object);
  }

  [Fact]
  public async Task ValidateToken_ReturnsValidTrue_WhenTokenIsValid()
  {
    // arrange
    var command = new ValidateTokenCommand(new ValidateTokenRequest() { Token = "valid_token", TokenType = TokenType.PasswordReset }, CancellationToken.None);
    TokenServiceMock.Setup(x => x.IsTokenValidAsync("valid_token", TokenType.PasswordReset, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(true));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.True(result.Data.IsValid);
    TokenServiceMock.Verify(x => x.IsTokenValidAsync("valid_token", TokenType.PasswordReset, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task ValidateToken_ReturnsValidFalse_WhenTokenIsInvalid()
  {
    // arrange
    var command = new ValidateTokenCommand(new ValidateTokenRequest() { Token = "invalid_token", TokenType = TokenType.PasswordReset }, CancellationToken.None);
    TokenServiceMock.Setup(x => x.IsTokenValidAsync("invalid_token", TokenType.PasswordReset, It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(false));

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.False(result.Data.IsValid);
    TokenServiceMock.Verify(x => x.IsTokenValidAsync("invalid_token", TokenType.PasswordReset, It.IsAny<CancellationToken>()), Times.Once);
  }
}
