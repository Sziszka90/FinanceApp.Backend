using FinanceApp.Domain.Common;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class User : BaseEntity
{
  public string UserName { get; set; }
  public string Email { get; set; } = string.Empty;
  public bool IsEmailConfirmed { get; set; } = false;
  public string PasswordHash { get; set; }
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
}
