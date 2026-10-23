namespace FinanceApp.Backend.Domain.Options;

public class CacheSettings
{
  /// <summary>
  /// Gets or sets the connection string for the cache.
  /// </summary>
  public string ConnectionString { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the instance name for the cache.
  /// </summary>
  public string InstanceName { get; set; } = string.Empty;
}
