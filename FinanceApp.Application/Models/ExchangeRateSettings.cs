namespace FinanceApp.Application.Models;

public class ExchangeRateSettings
{
  /// <summary>
  /// Represents the settings for exchange rate API integration.
  /// </summary>
  public string Endpoint { get; set; } = string.Empty;

  /// <summary>
  /// The API key used for authenticating requests to the exchange rate API.
  /// </summary>
  public string ApiKey { get; set; } = string.Empty;
}
