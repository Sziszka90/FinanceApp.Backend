using FinanceApp.Domain.Common;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class User : BaseEntity
{
  /// <summary>
  /// User name of the user
  /// </summary>
  public string UserName { get; set; }

  /// <summary>
  /// Email of the user
  /// </summary>
  public string Email { get; set; } = string.Empty;

  /// <summary>
  /// Indicates whether the user's email is confirmed
  /// </summary>
  public bool IsEmailConfirmed { get; set; } = false;

  /// <summary>
  /// Password hash of the user
  /// </summary>
  public string PasswordHash { get; set; }

  /// <summary>
  /// Base currency of the user
  /// </summary>
  public CurrencyEnum BaseCurrency { get; set; }

  /// <summary>
  /// Reset password token for the user
  /// </summary>
  public string? ResetPasswordToken { get; set; }

  /// <summary>
  /// Expiration time for the reset password token
  /// </summary>
  public DateTimeOffset? ResetPasswordTokenExpiration { get; set; }

  /// <summary>
  /// Email confirmation token for the user
  /// </summary>
  public string? EmailConfirmationToken { get; set; }

  /// <summary>
  /// Expiration time for the email confirmation token
  /// </summary>
  public DateTimeOffset? EmailConfirmationTokenExpiration { get; set; }

  public User(string userName, string email, string passwordHash, CurrencyEnum baseCurrency)
  {
    UserName = userName;
    Email = email;
    PasswordHash = passwordHash;
    BaseCurrency = baseCurrency;
  }

  /// <summary>
  /// ONLY FOR TESTING PURPOSES
  /// </summary>
  internal User(string userName, string email, bool isEmailConfirmed, string passwordHash, CurrencyEnum baseCurrency)
  {
    UserName = userName;
    Email = email;
    IsEmailConfirmed = isEmailConfirmed;
    PasswordHash = passwordHash;
    BaseCurrency = baseCurrency;
  }

  public void Update(string? userName, string? passwordHash, CurrencyEnum? baseCurrency)
  {
    UserName = userName ?? UserName;
    PasswordHash = passwordHash ?? PasswordHash;
    BaseCurrency = baseCurrency ?? BaseCurrency;
  }

  public void UpdatePassword(string passwordHash)
  {
    PasswordHash = passwordHash;
  }
}
