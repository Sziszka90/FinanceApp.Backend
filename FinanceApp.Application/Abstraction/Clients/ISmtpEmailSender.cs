using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ISmtpEmailSender
{
  /// <summary>
  /// Sends an email confirmation to the user.
  /// </summary>
  /// <param name="user"></param>
  /// <returns></returns>
  Task SendEmailConfirmationAsync(User user);

  /// <summary>
  /// Sends a password reset email to the user.
  /// </summary>
  /// <param name="email"></param>
  /// <returns></returns>
  Task SendForgotPasswordAsync(string email);
}
