using System.Net;
using System.Net.Mail;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using Microsoft.Extensions.Options;

public class SmtpEmailSender : ISmtpEmailSender
{
  private readonly SmtpSettings _smtpSettings;
  private readonly IJwtService _jwtService;

  public SmtpEmailSender(IOptions<SmtpSettings> smtpOptions, IJwtService jwtService)
  {
    _jwtService = jwtService;
    _smtpSettings = smtpOptions.Value;
  }
  public async Task SendEmailConfirmationAsync(User user)
  {
    using var client = new SmtpClient(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort)
    {
      Credentials = new NetworkCredential(_smtpSettings.SmtpUser, _smtpSettings.SmtpPass),
      EnableSsl = true
    };

    var confirmationToken = _jwtService.GenerateToken(user.UserName);
    var confirmationLink = $"https://financeapp.fun/api/users/{user.Id}/confirm-email?token={confirmationToken}";

    var model = new
    {
      user.UserName,
      ConfirmationLink = confirmationLink,
    };

    var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clients", "EmailConfirmationTemplate.html");
    string template = await File.ReadAllTextAsync(templatePath);

    // Replace placeholders in the template
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

    await client.SendMailAsync(mailMessage);
  }

  public async Task SendResetPasswordAsync(string email)
  {
    using var client = new SmtpClient(_smtpSettings.SmtpHost, _smtpSettings.SmtpPort)
    {
      Credentials = new NetworkCredential(_smtpSettings.SmtpUser, _smtpSettings.SmtpPass),
      EnableSsl = true
    };

    var resetPasswordToken = _jwtService.GenerateToken(email);
    var resetPasswordLink = $"https://financeapp.fun/reset-password?token={resetPasswordToken}";

    var model = new
    {
      ResetPasswordLink = resetPasswordLink,
    };

    var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clients", "EmailResetPasswordTemplate.html");
    string template = await File.ReadAllTextAsync(templatePath);

    // Replace placeholders in the template
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

    await client.SendMailAsync(mailMessage);
  }
}
