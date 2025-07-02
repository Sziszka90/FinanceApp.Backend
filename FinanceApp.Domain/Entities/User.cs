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

  public User(string userName, string email, string passwordHash, CurrencyEnum baseCurrency)
  {
    UserName = userName;
    Email = email;
    PasswordHash = passwordHash;
    BaseCurrency = baseCurrency;
  }

  public void Update(string userName, string passwordHash, CurrencyEnum baseCurrency)
  {
    UserName = userName;
    PasswordHash = passwordHash;
    BaseCurrency = baseCurrency;
  }

  public void UpdatePassword(string passwordHash)
  {
    PasswordHash = passwordHash;
  }
}
