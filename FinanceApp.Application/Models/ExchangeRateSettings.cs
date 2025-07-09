namespace FinanceApp.Application.Models;

public class ExchangeRateSettings
{
  /// <summary>
  /// Represents the settings for exchange rate API integration.
  /// </summary>
  public string ApiUrl { get; set; } = string.Empty;

  /// <summary>
  /// The application ID used for authenticating requests to the exchange rate API.
  /// </summary>
  public string AppId { get; set; } = string.Empty;

  /// <summary>
  /// The API endpoint for fetching exchange rates.
  /// </summary>
  public string ApiEndpoint { get; set; } = string.Empty;
}
