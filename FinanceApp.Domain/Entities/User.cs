using FinanceApp.Domain.Common;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class User : BaseEntity
{
  #region Properties

  public string UserName { get; set; }
  public string PasswordHash { get; set; }
  public CurrencyEnum BaseCurrency { get; set; }

  #endregion

  #region Constructors

  public User(string userName, string passwordHash, CurrencyEnum baseCurrency)
  {
    UserName = userName;
    PasswordHash = passwordHash;
    BaseCurrency = baseCurrency;
  }

  #endregion

  #region Methods

  public void Update(string userName, string passwordHash, CurrencyEnum baseCurrency)
  {
    UserName = userName;
    PasswordHash = passwordHash;
    BaseCurrency = baseCurrency;
  }

  #endregion
}
