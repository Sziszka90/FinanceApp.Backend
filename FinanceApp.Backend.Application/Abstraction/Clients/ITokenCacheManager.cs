using Microsoft.Extensions.Caching.Distributed;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface ITokenCacheManager
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
  /// Invalidates a token by marking it as invalid in the cache.
  /// </summary>
  /// <param name="token">The token to invalidate.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task InvalidateLoginTokenAsync(string token);

  /// <summary>
  /// Checks if a token is invalid (either explicitly invalidated or not found).
  /// </summary>
  /// <param name="token"></param>
  /// <returns>Boolean indicating whether the token is invalid.</returns>
  Task<bool> IsLoginTokenInvalidAsync(string token);

  /// <summary>
  /// Checks if a token is valid.
  /// </summary>
  /// <param name="token">The token to check.</param>
  /// <returns>Boolean indicating whether the token is valid.</returns>
  Task<bool> IsLoginTokenValidAsync(string token);

  /// <summary>
  /// Checks if a refresh token is valid.
  /// </summary>
  /// <param name="token">The token to check.</param>
  /// <returns>Boolean indicating whether the refresh token is valid.</returns>
  Task<bool> IsRefreshTokenValidAsync(string token);

  /// <summary>
  /// Saves a token in the cache.
  /// </summary>
  /// <param name="token">The token to cache.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task SaveLoginTokenAsync(string token);

  /// <summary>
  /// Saves an email confirmation token in the cache with 24-hour expiration.
  /// </summary>
  /// <param name="token">The email confirmation token to cache.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task SaveEmailConfirmationTokenAsync(string token);

  /// <summary>
  /// Saves a refresh token in the cache with 24-hour expiration.
  /// </summary>
  /// <param name="token">The refresh token to cache.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task SaveRefreshTokenAsync(string token);

  /// <summary>
  /// Invalidates an email confirmation token by marking it as invalid in the cache.
  /// </summary>
  /// <param name="token">The email confirmation token to invalidate.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task InvalidateEmailConfirmationTokenAsync(string token);

  /// <summary>
  /// Checks if an email confirmation token is valid.
  /// </summary>
  /// <param name="token">The email confirmation token to check.</param>
  /// <returns>Boolean indicating whether the token is valid.</returns>
  Task<bool> IsEmailConfirmationTokenValidAsync(string token);

  /// <summary>
  /// Checks if an email confirmation token is invalid (either explicitly invalidated or not found).
  /// </summary>
  /// <param name="token">The email confirmation token to check.</param>
  /// <returns>Boolean indicating whether the token is invalid.</returns>
  Task<bool> IsEmailConfirmationTokenInvalidAsync(string token);

  /// <summary>
  /// Saves a password reset token in the cache with 1-hour expiration.
  /// </summary>
  /// <param name="token">The password reset token to cache.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task SavePasswordResetTokenAsync(string token);

  /// <summary>
  /// Invalidates a password reset token by marking it as invalid in the cache.
  /// </summary>
  /// <param name="token">The password reset token to invalidate.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task InvalidatePasswordResetTokenAsync(string token);

  /// <summary>
  /// Invalidates a refresh token by marking it as invalid in the cache.
  /// </summary>
  /// <param name="token">The refresh token to invalidate.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task InvalidateRefreshTokenAsync(string token);

  /// <summary>
  /// Checks if a password reset token is valid.
  /// </summary>
  /// <param name="token">The password reset token to check.</param>
  /// <returns>Boolean indicating whether the token is valid.</returns>
  Task<bool> IsPasswordResetTokenValidAsync(string token);

  /// <summary>
  /// Checks if a password reset token is invalid (either explicitly invalidated or not found).
  /// </summary>
  /// <param name="token">The password reset token to check.</param>
  /// <returns>Boolean indicating whether the token is invalid.</returns>
  Task<bool> IsPasswordResetTokenInvalidAsync(string token);

  /// <summary>
  /// Checks if a token exists in the cache (regardless of its validity).
  /// </summary>
  /// <param name="token">The token to check for existence.</param>
  /// <returns>Boolean indicating whether the token exists in cache.</returns>
  Task<bool> LoginTokenExistsAsync(string token);

  /// <summary>
  /// Checks if an email confirmation token exists in the cache (regardless of its validity).
  /// </summary>
  /// <param name="token">The email confirmation token to check for existence.</param>
  /// <returns>Boolean indicating whether the token exists in cache.</returns>
  Task<bool> EmailConfirmationTokenExistsAsync(string token);

  /// <summary>
  /// Checks if a password reset token exists in the cache (regardless of its validity).
  /// </summary>
  /// <param name="token">The password reset token to check for existence.</param>
  /// <returns>Boolean indicating whether the token exists in cache.</returns>
  Task<bool> PasswordResetTokenExistsAsync(string token);

  /// <summary>
  /// Checks if a refresh token exists in the cache (regardless of its validity).
  /// </summary>
  /// <param name="token">The refresh token to check for existence.</param>
  /// <returns>Boolean indicating whether the token exists in cache.</returns>
  Task<bool> RefreshTokenExistsAsync(string token);

  /// <summary>
  /// Removes a value from the cache.
  /// </summary>
  /// <param name="key">The key for the cache entry.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  /// <exception cref="CacheException">Thrown when there is an error removing the cache value.</exception>
  Task RemoveAsync(string key);
}
