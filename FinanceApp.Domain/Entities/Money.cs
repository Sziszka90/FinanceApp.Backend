using FinanceApp.Domain.Enums;

namespace FinanceApp.Domain.Entities;

public class Money
{
  #region Properties

  /// <summary>
  /// Currency of the Money
  /// </summary>
  public CurrencyEnum Currency { get; set; }

  /// <summary>
  /// Amount of the Money
  /// </summary>
  public decimal Amount { get; set; }

  #endregion
}