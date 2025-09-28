using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Infrastructure.Cache;

public class TokenCacheManager : ITokenCacheManager
{

  private readonly ILogger<TokenCacheManager> _logger;
  private readonly IDistributedCache _cache;

  public TokenCacheManager(ILogger<TokenCacheManager> logger, IDistributedCache cache)
  {
    _logger = logger;
    _cache = cache;
  }

  public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null, CancellationToken cancellationToken = default)
  {
    try
    {
      var json = JsonSerializer.Serialize(value);
      var cacheOptions = options ?? new DistributedCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
      };
      await _cache.SetStringAsync(key, json, cacheOptions, cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to set cache value for key: {Key}", key);
      throw new CacheException("SET", key, ex);
    }
  }

  public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
  {
    try
    {
      var json = await _cache.GetStringAsync(key, cancellationToken);
      return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to get cache value for key: {Key}", key);
      throw new CacheException("GET", key, ex);
    }
  }

  public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
  {
    try
    {
      await _cache.RemoveAsync(key, cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to remove cache value for key: {Key}", key);
      throw new CacheException("REMOVE", key, ex);
    }
  }

  public async Task SaveEmailConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    await SetAsync($"EmailToken:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
    }, cancellationToken);
  }

  public async Task SavePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    await SetAsync($"PasswordResetToken:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
    }, cancellationToken);
  }

  public async Task SaveLoginTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    await SetAsync($"LoginToken:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    }, cancellationToken);
  }

  public Task SaveRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    return SetAsync($"RefreshToken:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
    }, cancellationToken);
  }

  public async Task<bool> IsEmailConfirmationTokenValidAsync(string token, CancellationToken cancellationToken = default)
  {
    return await GetAsync<bool>($"EmailToken:{token}", cancellationToken);
  }

  public async Task<bool> IsEmailConfirmationTokenInvalidAsync(string token, CancellationToken cancellationToken = default)
  {
    var isValid = await IsEmailConfirmationTokenValidAsync(token, cancellationToken);
    return !isValid;
  }

  public async Task<bool> IsPasswordResetTokenValidAsync(string token, CancellationToken cancellationToken = default)
  {
    return await GetAsync<bool>($"PasswordResetToken:{token}", cancellationToken);
  }

  public async Task<bool> IsPasswordResetTokenInvalidAsync(string token, CancellationToken cancellationToken = default)
  {
    var isValid = await IsPasswordResetTokenValidAsync(token, cancellationToken);
    return !isValid;
  }

  public async Task<bool> IsLoginTokenValidAsync(string token, CancellationToken cancellationToken = default)
  {
    return await GetAsync<bool>($"LoginToken:{token}", cancellationToken);
  }

  public async Task<bool> IsLoginTokenInvalidAsync(string token, CancellationToken cancellationToken = default)
  {
    var isValid = await IsLoginTokenValidAsync(token, cancellationToken);
    return !isValid;
  }

  public Task<bool> IsRefreshTokenValidAsync(string token, CancellationToken cancellationToken = default)
  {
    return GetAsync<bool>($"RefreshToken:{token}", cancellationToken);
  }

  public async Task InvalidateEmailConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    await SetAsync($"EmailToken:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
    }, cancellationToken);
  }

  public async Task InvalidatePasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    await SetAsync($"PasswordResetToken:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
    }, cancellationToken);
  }

  public async Task InvalidateLoginTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    await SetAsync($"LoginToken:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
    }, cancellationToken);
  }

  public Task InvalidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    return SetAsync($"RefreshToken:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(60)
    }, cancellationToken);
  }

  public async Task<bool> LoginTokenExistsAsync(string token, CancellationToken cancellationToken = default)
  {
    var json = await GetCacheStringAsync($"LoginToken:{token}", cancellationToken);
    return json is not null;
  }

  public async Task<bool> EmailConfirmationTokenExistsAsync(string token, CancellationToken cancellationToken = default)
  {
    var json = await GetCacheStringAsync($"EmailToken:{token}", cancellationToken);
    return json is not null;
  }

  public async Task<bool> PasswordResetTokenExistsAsync(string token, CancellationToken cancellationToken = default)
  {
    var json = await GetCacheStringAsync($"PasswordResetToken:{token}", cancellationToken);
    return json is not null;
  }

  public async Task<bool> RefreshTokenExistsAsync(string token, CancellationToken cancellationToken = default)
  {
    var json = await GetCacheStringAsync($"RefreshToken:{token}", cancellationToken);
    return json is not null;
  }

  private async Task<string?> GetCacheStringAsync(string key, CancellationToken cancellationToken = default)
  {
    try
    {
      return await _cache.GetStringAsync(key, cancellationToken);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to get cache string for key: {Key}", key);
      throw new CacheException("GET_STRING", key, ex);
    }
  }
}
