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
  public async Task SendEmailAsync(User user)
  {
    using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
    {
      Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password),
      EnableSsl = true
    };

    var confirmationToken = _jwtService.GenerateToken(user.UserName);
    var confirmationLink = $"https://financeapp.fun/api/users/{user.Id}/confirm-email?token={confirmationToken}";

    var model = new
    {
      UserName = user.UserName,
      ConfirmationLink = confirmationLink,
    };

    var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Clients", "EmailTemplate.html");
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
}
