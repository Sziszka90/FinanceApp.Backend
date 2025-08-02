using System.Net.Mail;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Clients;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Domain.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.ClientTests;

public class SmtpEmailSenderTests
{
  private SmtpEmailSender CreateSender(Mock<SmtpClient>? smtpClientMock = null)
  {
    var loggerMock = new Mock<ILogger<ISmtpEmailSender>>();
    var smtpSettings = new SmtpSettings
    {
      SmtpHost = "smtp.test.com",
      SmtpPort = 587,
      SmtpUser = "user",
      SmtpPass = "pass",
      FromEmail = "from@test.com"
    };
    var options = Options.Create(smtpSettings);
    return new SmtpEmailSender(loggerMock.Object, options);
  }

  [Fact]
  public async Task SendEmailConfirmationAsync_ReturnsFailure_WhenTemplateMissing()
  {
    // arrange
    var sender = CreateSender();
    var user = new User("TestUser", "test@example.com", "hash", CurrencyEnum.USD);
    var token = "token";
    // Simulate missing template file
    var originalBaseDir = AppDomain.CurrentDomain.BaseDirectory;
    var templatePath = Path.Combine(originalBaseDir, "Clients/EmailTemplates", "EmailConfirmationTemplate.html");
    if (File.Exists(templatePath)) File.Delete(templatePath);

    // act
    var result = await sender.SendEmailConfirmationAsync(user, token);

    // assert
    Assert.False(result.IsSuccess);
  }

  [Fact]
  public async Task SendForgotPasswordAsync_ReturnsFailure_WhenTemplateMissing()
  {
    // arrange
    var sender = CreateSender();
    var email = "test@example.com";
    var token = "token";
    // Simulate missing template file
    var originalBaseDir = AppDomain.CurrentDomain.BaseDirectory;
    var templatePath = Path.Combine(originalBaseDir, "Clients/EmailTemplates", "ForgotPasswordTemplate.html");
    if (File.Exists(templatePath)) File.Delete(templatePath);

    // act
    var result = await sender.SendForgotPasswordAsync(email, token);

    // assert
    Assert.False(result.IsSuccess);
  }
}
