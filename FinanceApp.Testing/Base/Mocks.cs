using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FinanceApp.Testing.Base;

public static class Mocks
{
  public static void RegisterEmailServiceMock(this IServiceCollection services)
  {
    var emailServiceMock = new Mock<ISmtpEmailSender>();

    emailServiceMock
        .Setup(x => x.SendEmailConfirmationAsync(It.IsAny<User>()))
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
}
