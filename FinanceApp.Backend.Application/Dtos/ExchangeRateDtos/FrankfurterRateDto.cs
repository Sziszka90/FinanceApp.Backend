namespace FinanceApp.Backend.Application.Dtos.ExchangeRateDtos;

/// <summary>One base/quote observation returned by Frankfurter's v2 rates endpoint.</summary>
public class FrankfurterRateDto
{
  public string Date { get; set; } = string.Empty;
  public string? Base { get; set; }
  public string Quote { get; set; } = string.Empty;
  public decimal Rate { get; set; }
}
