using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ISmtpEmailSender
{
  /// <summary>
  /// Sends an email confirmation to the user.
  /// </summary>
  /// <param name="user"></param>
  /// <returns>Boolean - success or failure</returns>
  Task<Result<bool>> SendEmailConfirmationAsync(User user);

  /// <summary>
  /// Sends a password reset email to the user.
  /// </summary>
  /// <param name="email"></param>
  /// <returns>Boolean - success or failure</returns>
  Task<Result<bool>> SendForgotPasswordAsync(string email);
}
