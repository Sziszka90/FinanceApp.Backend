using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FinanceApp.Backend.Testing.Base;

public static class Mocks
{
  public static void RegisterEmailServiceMock(this IServiceCollection services)
  {
    var emailServiceMock = new Mock<ISmtpEmailSender>();

    emailServiceMock
        .Setup(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), It.IsAny<string>()))
        .ReturnsAsync(Result.Success(true));

    services.AddSingleton(emailServiceMock.Object);
  }

  public static void RegisterBcryptMock(this IServiceCollection services)
  {
    var bcryptMock = new Mock<IBcryptService>();
    bcryptMock.Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
    bcryptMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashed_password");
    services.AddSingleton(bcryptMock.Object);
  }

  public static void RegisterJwtMock(this IServiceCollection services)
  {
    var jwtMock = new Mock<IJwtService>();
    jwtMock.Setup(x => x.GenerateToken(It.IsAny<string>())).Returns("mocked_jwt_token");
    jwtMock.Setup(x => x.ValidateToken(It.IsAny<string>())).Returns(true);
    jwtMock.Setup(x => x.GetUserEmailFromToken(It.IsAny<string>())).Returns("test_user90@example.com");

    services.AddSingleton(jwtMock.Object);
  }

  public static void RegisterCacheManagerMock(this IServiceCollection services)
  {
    var cacheManagerMock = new Mock<ICacheManager>();
    cacheManagerMock.Setup(x => x.IsPasswordResetTokenValidAsync(It.IsAny<string>())).ReturnsAsync(true);
    cacheManagerMock.Setup(x => x.IsEmailConfirmationTokenValidAsync(It.IsAny<string>())).ReturnsAsync(true);
    cacheManagerMock.Setup(x => x.IsTokenValidAsync(It.IsAny<string>())).ReturnsAsync(true);

    cacheManagerMock.Setup(x => x.PasswordResetTokenExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
    cacheManagerMock.Setup(x => x.EmailConfirmationTokenExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
    cacheManagerMock.Setup(x => x.TokenExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

    cacheManagerMock.Setup(x => x.InvalidatePasswordResetTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
    cacheManagerMock.Setup(x => x.InvalidateEmailConfirmationTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
    cacheManagerMock.Setup(x => x.InvalidateTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

    cacheManagerMock.Setup(x => x.SaveEmailConfirmationTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
    cacheManagerMock.Setup(x => x.SaveTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
    cacheManagerMock.Setup(x => x.SavePasswordResetTokenAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

    services.AddSingleton(cacheManagerMock.Object);
  }
}
