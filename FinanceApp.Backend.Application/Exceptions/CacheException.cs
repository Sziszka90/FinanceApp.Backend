namespace FinanceApp.Backend.Application.Exceptions;

public class CacheException : Exception
{
  public string? CacheKey { get; }
  public string Operation { get; }

  public CacheException(string operation, string? cacheKey = null)
    : base($"Cache operation '{operation}' failed" + (cacheKey != null ? $" for key '{cacheKey}'" : ""))
  {
    Operation = operation;
    CacheKey = cacheKey;
  }

  public CacheException(string operation, string? cacheKey, Exception innerException)
    : base($"Cache operation '{operation}' failed" + (cacheKey != null ? $" for key '{cacheKey}'" : ""), innerException)
  {
    Operation = operation;
    CacheKey = cacheKey;
  }

  public CacheException(string message, Exception innerException) : base(message, innerException)
  {
    Operation = "Unknown";
  }
}
