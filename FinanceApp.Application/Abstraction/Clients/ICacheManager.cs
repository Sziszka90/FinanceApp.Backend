using Microsoft.Extensions.Caching.Distributed;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ICacheManager
{
  /// <summary>
  /// Sets a value in the cache.
  /// </summary>
  /// <param name="key">The key for the cache entry.</param>
  /// <param name="value">The value to cache.</param>
  /// <param name="options">Optional cache entry options.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null);

  /// <summary>
  /// Gets a value from the cache.
  /// </summary>
  /// <param name="key">The key for the cache entry.</param>
  /// <returns>The cached value, or null if not found.</returns>
  Task<T?> GetAsync<T>(string key);

  /// <summary>
  /// Invalidates a token in the cache.
  /// </summary>
  /// <param name="token"></param>
  /// <returns>Boolean indicating whether the token is invalid.</returns>
  Task<bool> IsTokenInvalidAsync(string token);

  /// <summary>
  /// Saves a token in the cache.
  /// </summary>
  /// <param name="token">The token to cache.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task SaveTokenAsync(string token);

  /// <summary>
  /// Removes a value from the cache.
  /// </summary>
  /// <param name="key">The key for the cache entry.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task RemoveAsync(string key);
}
