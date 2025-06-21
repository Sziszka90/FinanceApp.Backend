using FinanceApp.Domain.Entities;

public interface ISmtpEmailSender
{
  Task SendEmailAsync(User user);
}
