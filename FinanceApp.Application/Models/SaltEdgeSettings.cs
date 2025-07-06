namespace FinanceApp.Application.Models;

public class SaltEdgeSettings
{
  /// <summary>
  /// App ID for Salt Edge integration.
  /// </summary>
  public string AppId { get; set; } = string.Empty;

  /// <summary>
  /// The secret for Salt Edge integration.
  /// </summary>
  public string Secret { get; set; } = string.Empty;

  /// <summary>
  /// The URL for Salt Edge integration.
  /// </summary>
  public string BaseUrl { get; set; } = string.Empty;
}
