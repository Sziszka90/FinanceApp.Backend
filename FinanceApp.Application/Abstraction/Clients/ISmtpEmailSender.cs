using FinanceApp.Domain.Entities;

public interface ISmtpEmailSender
{
  Task SendEmailConfirmationAsync(User user);
  Task SendForgotPasswordAsync(string email);
}
