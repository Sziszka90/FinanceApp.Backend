using FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.AuthTests;

public class ValidateTokenCommandHandlerTests : TestBase
{
  private readonly Mock<ILogger<ValidateTokenCommandHandler>> _loggerMock = new();
  private readonly Mock<ITokenService> _tokenServiceMock = new();
  private readonly ValidateTokenCommandHandler _handler;

  public ValidateTokenCommandHandlerTests()
  {
    _handler = new ValidateTokenCommandHandler(_loggerMock.Object, _tokenServiceMock.Object);
  }

  [Fact]
  public async Task ValidateToken_ReturnsValidTrue_WhenTokenIsValid()
  {
    // arrange
    var command = new ValidateTokenCommand("valid_token", CancellationToken.None);
    _tokenServiceMock.Setup(x => x.IsTokenValidAsync("valid_token", TokenType.PasswordReset)).ReturnsAsync(true);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.True(result.Data.IsValid);
    _tokenServiceMock.Verify(x => x.IsTokenValidAsync("valid_token", TokenType.PasswordReset), Times.Once);
  }

  [Fact]
  public async Task ValidateToken_ReturnsValidFalse_WhenTokenIsInvalid()
  {
    // arrange
    var command = new ValidateTokenCommand("invalid_token", CancellationToken.None);
    _tokenServiceMock.Setup(x => x.IsTokenValidAsync("invalid_token", TokenType.PasswordReset)).ReturnsAsync(false);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.False(result.Data.IsValid);
    _tokenServiceMock.Verify(x => x.IsTokenValidAsync("invalid_token", TokenType.PasswordReset), Times.Once);
  }
}
