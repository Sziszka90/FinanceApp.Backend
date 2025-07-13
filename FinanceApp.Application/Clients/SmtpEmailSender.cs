using System.Net;
using System.Net.Mail;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Models;
using FinanceApp.Application.Models.Options;
using FinanceApp.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class SmtpEmailSender : ISmtpEmailSender
{
  private readonly ILogger<ISmtpEmailSender> _logger;
  private readonly IJwtService _jwtService;
  private readonly SmtpSettings _smtpSettings;

  public SmtpEmailSender(
    ILogger<ISmtpEmailSender> logger,
    IJwtService jwtService,
    IOptions<SmtpSettings> smtpOptions)
  {
    _logger = logger;
    _jwtService = jwtService;
    _smtpSettings = smtpOptions.Value;
  }
  public async Task<Result<bool>> SendEmailConfirmationAsync(User user)
  {
    using var client = new SmtpClient(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort)
    {
      Credentials = new NetworkCredential(_smtpSettings.SmtpUser, _smtpSettings.SmtpPass),
      EnableSsl = true
    };

    var confirmationToken = _jwtService.GenerateToken(user.UserName);
    var confirmationLink = $"https://www.financeapp.fun/api/users/{user.Id}/confirm-email?token={confirmationToken}";

    var model = new
    {
      user.UserName,
      ConfirmationLink = confirmationLink,
    };

    var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clients/EmailTemplates", "EmailConfirmationTemplate.html");
    string template = await File.ReadAllTextAsync(templatePath);

    string body = template
      .Replace("@Model.UserName", user.UserName)
      .Replace("@Model.ConfirmationLink", model.ConfirmationLink)
      .Replace("@DateTime.Now.Year", DateTime.Now.Year.ToString());

    var mailMessage = new MailMessage
    {
      From = new MailAddress(_smtpSettings.FromEmail),
      Subject = "Email Confirmation",
      Body = body,
      IsBodyHtml = true,
    };

    mailMessage.To.Add(user.Email);

    try
    {
      await client.SendMailAsync(mailMessage);
      _logger.LogDebug("Email confirmation sent to {Email}", user.Email);
      return Result.Success(true);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error occurred while sending email confirmation to {Email}", user.Email);
      return Result.Failure<bool>(ApplicationError.ExternalCallError("Email confirmation failed."));
    }
  }

  public async Task<Result<bool>> SendForgotPasswordAsync(string email)
  {
    using var client = new SmtpClient(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort)
    {
      Credentials = new NetworkCredential(_smtpSettings.SmtpUser, _smtpSettings.SmtpPass),
      EnableSsl = true
    };

    var resetPasswordToken = _jwtService.GenerateToken(email);
    var resetPasswordLink = $"https://www.financeapp.fun/reset-password?token={resetPasswordToken}";

    var model = new
    {
      ResetPasswordLink = resetPasswordLink,
    };

    var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clients/EmailTemplates", "ForgotPasswordTemplate.html");
    string template = await File.ReadAllTextAsync(templatePath);

    string body = template
      .Replace("@Model.ResetPasswordLink", model.ResetPasswordLink)
      .Replace("@DateTime.Now.Year", DateTime.Now.Year.ToString());

    var mailMessage = new MailMessage
    {
      From = new MailAddress(_smtpSettings.FromEmail),
      Subject = "Email Reset Password",
      Body = body,
      IsBodyHtml = true,
    };

    mailMessage.To.Add(email);

    try
    {
      await client.SendMailAsync(mailMessage);
      _logger.LogDebug("Password reset email sent to {Email}", email);
      return Result.Success(true);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error occurred while sending password reset email to {Email}", email);
      return Result.Failure<bool>(ApplicationError.ExternalCallError("Password reset email sending failed."));
    }

  }
}
