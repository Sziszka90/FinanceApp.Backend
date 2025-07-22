using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface ISmtpEmailSender
{
  /// <summary>
  /// Sends an email confirmation to the user.
  /// </summary>
  /// <param name="user"></param>
  /// <param name="confirmationToken"></param>
  /// <returns>Boolean - success or failure</returns>
  Task<Result<bool>> SendEmailConfirmationAsync(User user, string confirmationToken);

  /// <summary>
  /// Sends a password reset email to the user.
  /// </summary>
  /// <param name="email"></param>
  /// <param name="resetPasswordToken"></param>
  /// <returns>Boolean - success or failure</returns>
  Task<Result<bool>> SendForgotPasswordAsync(string email, string resetPasswordToken);
}
