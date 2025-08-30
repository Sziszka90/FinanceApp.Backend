using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FinanceApp.Backend.Testing.Api.Base;

public static class Mocks
{
  public static void RegisterEmailServiceMock(this IServiceCollection services)
  {
    var emailServiceMock = new Mock<ISmtpEmailSender>();

    emailServiceMock
        .Setup(x => x.SendEmailConfirmationAsync(It.IsAny<User>(), It.IsAny<string>()))
        .ReturnsAsync(Result.Success(true));

    emailServiceMock
        .Setup(x => x.SendForgotPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
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

  public static void RegisterTokenServiceMock(this IServiceCollection services)
  {
    var tokenServiceMock = new Mock<ITokenService>();

    tokenServiceMock
        .Setup(x => x.ValidateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
        .ReturnsAsync(Result.Success(false));

    tokenServiceMock
        .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), TokenType.Login))
        .ReturnsAsync(Result.Success("mock_login_token"));

    tokenServiceMock
        .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), TokenType.PasswordReset))
        .ReturnsAsync(Result.Success("mock_password_reset_token"));

    tokenServiceMock
        .Setup(x => x.GenerateTokenAsync(It.IsAny<string>(), TokenType.EmailConfirmation))
        .ReturnsAsync(Result.Success("mock_email_confirmation_token"));

    tokenServiceMock
        .Setup(x => x.ValidateTokenAsync("mock_login_token", TokenType.Login))
        .ReturnsAsync(Result.Success(true));

    tokenServiceMock
        .Setup(x => x.ValidateTokenAsync("mock_password_reset_token", TokenType.PasswordReset))
        .ReturnsAsync(Result.Success(true));

    tokenServiceMock
        .Setup(x => x.ValidateTokenAsync("mock_email_confirmation_token", TokenType.EmailConfirmation))
        .ReturnsAsync(Result.Success(true));

    tokenServiceMock
        .Setup(x => x.ValidateTokenAsync("invalid-token", It.IsAny<TokenType>()))
        .ReturnsAsync(Result.Success(false));

    tokenServiceMock
        .Setup(x => x.IsTokenValidAsync("mock_login_token", TokenType.Login))
        .ReturnsAsync(true);

    tokenServiceMock
        .Setup(x => x.IsTokenValidAsync("mock_password_reset_token", TokenType.PasswordReset))
        .ReturnsAsync(true);

    tokenServiceMock
        .Setup(x => x.IsTokenValidAsync("mock_email_confirmation_token", TokenType.EmailConfirmation))
        .ReturnsAsync(true);

    tokenServiceMock
        .Setup(x => x.IsTokenValidAsync("invalid_token", It.IsAny<TokenType>()))
        .ReturnsAsync(false);

    tokenServiceMock
        .Setup(x => x.GetEmailFromTokenAsync(It.IsAny<string>()))
        .Returns("test_user90@example.com");

    tokenServiceMock
        .Setup(x => x.InvalidateTokenAsync(It.IsAny<string>(), It.IsAny<TokenType>()))
        .Returns(Task.CompletedTask);

    services.AddSingleton(tokenServiceMock.Object);
  }

  public static void RegisterLLMProcessorClientMock(this IServiceCollection services)
  {
    var llmProcessorClientMock = new Mock<ILLMProcessorClient>();

    llmProcessorClientMock
        .Setup(x => x.MatchTransactionGroup(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<string>()))
        .ReturnsAsync(Result.Success(true));

    services.AddSingleton(llmProcessorClientMock.Object);
  }
}
