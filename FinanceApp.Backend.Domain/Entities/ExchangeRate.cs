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

  public ExchangeRate(
    string baseCurrency,
    string targetCurrency,
    decimal rate)
  {
    BaseCurrency = baseCurrency;
    TargetCurrency = targetCurrency;
    Rate = rate;
  }

  protected ExchangeRate() { }
}


