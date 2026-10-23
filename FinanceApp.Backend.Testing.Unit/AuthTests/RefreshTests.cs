using FinanceApp.Backend.Application.AuthApi.AuthCommands.Refresh;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using RTools_NTS.Util;

namespace FinanceApp.Backend.Testing.Unit.AuthTests;

public class RefreshTests : TestBase
{
  private readonly RefreshCommandHandler _handler;
  private readonly Mock<ILogger<RefreshCommandHandler>> _loggerMock;

  public RefreshTests()
  {
    _loggerMock = new Mock<ILogger<RefreshCommandHandler>>();
    _handler = new RefreshCommandHandler(
      _loggerMock.Object,
      TokenServiceMock.Object
    );
  }

  [Fact]
  public async Task RefreshHandler_ValidToken_ReturnsSuccess()
  {
    // arrange
    var refreshToken = "valid-refresh-token";
    var user = new User(null, "testuser", "test@example.com", true, "hashed", CurrencyEnum.USD);
    TokenServiceMock.Setup(x => x.IsRefreshTokenValidAsync(refreshToken, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(true));
    UserServiceMock.Setup(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(user));
    TokenServiceMock.Setup(x => x.GenerateTokenAsync(user.Email, TokenType.Login, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success("new-token"));
    var command = new RefreshCommand(refreshToken, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal("new-token", result.Data);
    TokenServiceMock.Verify(x => x.IsRefreshTokenValidAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GetEmailFromToken(refreshToken), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(user.Email, TokenType.Login, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task RefreshHandler_InvalidToken_ReturnsFailure()
  {
    // arrange
    var refreshToken = "invalid-refresh-token";
    TokenServiceMock.Setup(x => x.IsRefreshTokenValidAsync(refreshToken, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(false));
    var command = new RefreshCommand(refreshToken, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("INVALID_TOKEN", result.ApplicationError.Code);
    TokenServiceMock.Verify(x => x.IsRefreshTokenValidAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
    UserServiceMock.Verify(x => x.GetActiveUserAsync(It.IsAny<CancellationToken>()), Times.Never);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task RefreshHandler_UserNotFound_ReturnsFailure()
  {
    // arrange
    var refreshToken = "valid-refresh-token";
    TokenServiceMock.Setup(x => x.IsRefreshTokenValidAsync(refreshToken, It.IsAny<CancellationToken>()))
      .ReturnsAsync(Result.Success(true));
    TokenServiceMock.Setup(x => x.GetEmailFromToken(refreshToken))
      .Returns(() => null!);
    var command = new RefreshCommand(refreshToken, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.NotNull(result.ApplicationError);
    Assert.Equal("USER_NOT_FOUND", result.ApplicationError.Code);
    TokenServiceMock.Verify(x => x.IsRefreshTokenValidAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
    TokenServiceMock.Verify(x => x.GetEmailFromToken(refreshToken), Times.Once);
    TokenServiceMock.Verify(x => x.GenerateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>(), It.IsAny<CancellationToken>()), Times.Never);
  }
}
