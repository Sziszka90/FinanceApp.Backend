using System.Threading.Tasks;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Services;
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
    var cacheManagerMock = new Mock<ICacheManager>();
    jwtServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(false);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.IsTokenValidAsync("invalid", TokenType.Login);

    // assert
    Assert.False(result);
  }

  [Fact]
  public async Task IsTokenValidAsync_ValidLoginToken_ReturnsTrue()
  {
    // arrange
    var loggerMock = new Mock<ILogger<TokenService>>();
    var jwtServiceMock = new Mock<IJwtService>();
    var cacheManagerMock = new Mock<ICacheManager>();
    jwtServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(true);
    cacheManagerMock.Setup(x => x.IsLoginTokenValidAsync(It.IsAny<string>())).ReturnsAsync(true);
    var service = new TokenService(loggerMock.Object, jwtServiceMock.Object, cacheManagerMock.Object);

    // act
    var result = await service.IsTokenValidAsync("valid", TokenType.Login);

    // assert
    Assert.True(result);
  }
}
