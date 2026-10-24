namespace FinanceApp.Backend.Domain.Options;

public class ExchangeRateSettings
{
  /// <summary>
  /// Represents the settings for exchange rate API integration.
  /// </summary>
  public string ApiUrl { get; set; } = string.Empty;

  /// <summary>
  /// The API endpoint for fetching exchange rates.
  /// </summary>
  public string ApiEndpoint { get; set; } = string.Empty;

  /// <summary>
  /// The API endpoint template for fetching historical exchange rates. Use {date} for YYYY-MM-DD.
  /// </summary>
  public string HistoricalEndpoint { get; set; } = string.Empty;
}
