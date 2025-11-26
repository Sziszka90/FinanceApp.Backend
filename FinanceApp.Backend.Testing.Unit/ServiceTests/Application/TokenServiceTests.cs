using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.ServiceTests.Application;

public class TokenServiceTests
{
  [Fact]
  public async Task IsTokenValidAsync_InvalidToken_ReturnsFalse()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    jwtServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(false);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.IsTokenValidAsync("invalid", TokenType.Login);

    // assert
    Assert.False(result.Data);
  }

  [Fact]
  public async Task IsTokenValidAsync_ValidLoginToken_ReturnsTrue()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    jwtServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(true);
    cacheManagerMock.Setup(x => x.IsLoginTokenValidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.IsTokenValidAsync("valid", TokenType.Login, CancellationToken.None);

    // assert
    Assert.True(result.Data);
  }

  [Fact]
  public async Task IsTokenValidAsync_ValidPasswordResetToken_ReturnsTrue()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    jwtServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(true);
    cacheManagerMock.Setup(x => x.IsPasswordResetTokenValidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.IsTokenValidAsync("valid", TokenType.PasswordReset, CancellationToken.None);

    // assert
    Assert.True(result.Data);
  }

  [Fact]
  public async Task IsTokenValidAsync_ValidEmailConfirmationToken_ReturnsTrue()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    jwtServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(true);
    cacheManagerMock.Setup(x => x.IsEmailConfirmationTokenValidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.IsTokenValidAsync("valid", TokenType.EmailConfirmation, CancellationToken.None);

    // assert
    Assert.True(result.Data);
  }

  [Fact]
  public async Task IsTokenValidAsync_InvalidTokenInCache_ReturnsFalse()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    jwtServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(true);
    cacheManagerMock.Setup(x => x.IsLoginTokenValidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.IsTokenValidAsync("valid", TokenType.Login, CancellationToken.None);

    // assert
    Assert.False(result.Data);
  }

  [Fact]
  public async Task GenerateTokenAsync_LoginToken_GeneratesAndSavesToken()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    var expectedToken = "generated-login-token";
    jwtServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>())).Returns(expectedToken);
    cacheManagerMock.Setup(x => x.LoginTokenExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.GenerateTokenAsync("test@example.com", TokenType.Login);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedToken, result.Data);
    cacheManagerMock.Verify(x => x.SaveLoginTokenAsync(expectedToken, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GenerateTokenAsync_PasswordResetToken_GeneratesAndSavesToken()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    var expectedToken = "generated-password-reset-token";
    jwtServiceMock.Setup(x => x.GeneratePasswordResetToken(It.IsAny<string>())).Returns(expectedToken);
    cacheManagerMock.Setup(x => x.PasswordResetTokenExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.GenerateTokenAsync("test@example.com", TokenType.PasswordReset);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedToken, result.Data);
    cacheManagerMock.Verify(x => x.SavePasswordResetTokenAsync(expectedToken, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GenerateTokenAsync_EmailConfirmationToken_GeneratesAndSavesToken()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    var expectedToken = "generated-email-confirmation-token";
    jwtServiceMock.Setup(x => x.GenerateEmailConfirmationToken(It.IsAny<string>())).Returns(expectedToken);
    cacheManagerMock.Setup(x => x.EmailConfirmationTokenExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.GenerateTokenAsync("test@example.com", TokenType.EmailConfirmation);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Equal(expectedToken, result.Data);
    cacheManagerMock.Verify(x => x.SaveEmailConfirmationTokenAsync(expectedToken, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task ValidateTokenAsync_ValidToken_InvalidatesToken()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    jwtServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(true);
    cacheManagerMock.Setup(x => x.IsLoginTokenValidAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.ValidateTokenAsync("valid-token", TokenType.Login);

    // assert
    Assert.True(result.IsSuccess);
    Assert.True(result.Data);
    cacheManagerMock.Verify(x => x.InvalidateLoginTokenAsync("valid-token", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task ValidateTokenAsync_InvalidToken_ReturnsFailure()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    jwtServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(false);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.ValidateTokenAsync("invalid-token", TokenType.Login);

    // assert
    Assert.False(result.IsSuccess);
  }

  [Fact]
  public void GetEmailFromToken_ReturnsEmail()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    var expectedEmail = "test@example.com";
    jwtServiceMock.Setup(x => x.GetUserEmailFromToken(It.IsAny<string>())).Returns(expectedEmail);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = service.GetEmailFromToken("token");

    // assert
    Assert.Equal(expectedEmail, result);
  }

  [Fact]
  public void GetEmailFromToken_WhenNull_ReturnsEmptyString()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    jwtServiceMock.Setup(x => x.GetUserEmailFromToken(It.IsAny<string>())).Returns((string?)null);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = service.GetEmailFromToken("token");

    // assert
    Assert.Equal(string.Empty, result);
  }

  [Fact]
  public async Task InvalidateTokenAsync_LoginToken_InvalidatesSuccessfully()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.InvalidateTokenAsync("token", TokenType.Login);

    // assert
    Assert.True(result.IsSuccess);
    cacheManagerMock.Verify(x => x.InvalidateLoginTokenAsync("token", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task InvalidateTokenAsync_PasswordResetToken_InvalidatesSuccessfully()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.InvalidateTokenAsync("token", TokenType.PasswordReset);

    // assert
    Assert.True(result.IsSuccess);
    cacheManagerMock.Verify(x => x.InvalidatePasswordResetTokenAsync("token", It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task InvalidateTokenAsync_EmailConfirmationToken_InvalidatesSuccessfully()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ITokenCacheManager>();
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.InvalidateTokenAsync("token", TokenType.EmailConfirmation);

    // assert
    Assert.True(result.IsSuccess);
    cacheManagerMock.Verify(x => x.InvalidateEmailConfirmationTokenAsync("token", It.IsAny<CancellationToken>()), Times.Once);
  }
}
