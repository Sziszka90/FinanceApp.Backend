using FinanceApp.Backend.Domain.Common;

namespace FinanceApp.Backend.Domain.Entities;

public class ExchangeRate : BaseEntity
{
  /// <summary>
  /// Base currency for the exchange rate
  /// </summary>
  public string BaseCurrency { get; set; } = string.Empty;

  /// <summary>
  /// Target currency for the exchange rate
  /// </summary>
  public string TargetCurrency { get; set; } = string.Empty;

  /// <summary>
  /// The exchange rate value
  /// </summary>
  public decimal Rate { get; set; } = 0;

  /// <summary>
  /// Flag indicating if the rate is actual or historical
  /// </summary>
  public bool Actual { get; set; } = true;

  /// <summary>
  /// The date from which the exchange rate is valid
  /// </summary>
  public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.UtcNow;

  /// <summary>
  /// The date until which the exchange rate is valid
  /// </summary>
  public DateTimeOffset? ValidTo { get; set; } = null;

  public ExchangeRate(
  string baseCurrency,
  string targetCurrency,
  decimal rate)
  {
    BaseCurrency = baseCurrency;
    TargetCurrency = targetCurrency;
    Rate = rate;
    Actual = true;
    ValidFrom = DateTimeOffset.UtcNow;
  }

  public void ExpireExchangeRate()
  {
    Actual = false;
    ValidTo = DateTimeOffset.UtcNow;
  }

  protected ExchangeRate() { }
}


