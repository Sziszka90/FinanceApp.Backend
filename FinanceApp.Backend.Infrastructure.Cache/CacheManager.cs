using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Exceptions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Infrastructure.Cache;

public class CacheManager : ICacheManager
{

  private readonly ILogger<CacheManager> _logger;
  private readonly IDistributedCache _cache;

  public CacheManager(ILogger<CacheManager> logger, IDistributedCache cache)
  {
    _logger = logger;
    _cache = cache;
  }

  public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null)
  {
    try
    {
      var json = JsonSerializer.Serialize(value);
      var cacheOptions = options ?? new DistributedCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
      };
      await _cache.SetStringAsync(key, json, cacheOptions);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to set cache value for key: {Key}", key);
      throw new CacheException("SET", key, ex);
    }
  }

  public async Task<T?> GetAsync<T>(string key)
  {
    try
    {
      var json = await _cache.GetStringAsync(key);
      return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to get cache value for key: {Key}", key);
      throw new CacheException("GET", key, ex);
    }
  }

  public async Task RemoveAsync(string key)
  {
    try
    {
      await _cache.RemoveAsync(key);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to remove cache value for key: {Key}", key);
      throw new CacheException("REMOVE", key, ex);
    }
  }

  public async Task SaveEmailConfirmationTokenAsync(string token)
  {
    await SetAsync($"EmailToken:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
    });
  }

  public async Task InvalidateEmailConfirmationTokenAsync(string token)
  {
    await SetAsync($"EmailToken:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
    });
  }

  public async Task<bool> IsEmailConfirmationTokenValidAsync(string token)
  {
    return await GetAsync<bool>($"EmailToken:{token}");
  }

  public async Task<bool> IsEmailConfirmationTokenInvalidAsync(string token)
  {
    var isValid = await IsEmailConfirmationTokenValidAsync(token);
    return !isValid;
  }

  public async Task SavePasswordResetTokenAsync(string token)
  {
    await SetAsync($"PasswordResetToken:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    });
  }

  public async Task InvalidatePasswordResetTokenAsync(string token)
  {
    await SetAsync($"PasswordResetToken:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    });
  }

  public async Task<bool> IsPasswordResetTokenValidAsync(string token)
  {
    return await GetAsync<bool>($"PasswordResetToken:{token}");
  }

  public async Task<bool> IsPasswordResetTokenInvalidAsync(string token)
  {
    var isValid = await IsPasswordResetTokenValidAsync(token);
    return !isValid;
  }

  public async Task InvalidateLoginTokenAsync(string token)
  {
    await SetAsync($"LoginToken:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    });
  }

  public async Task SaveLoginTokenAsync(string token)
  {
    await SetAsync($"LoginToken:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    });
  }

  public async Task<bool> IsLoginTokenValidAsync(string token)
  {
    return await GetAsync<bool>($"LoginToken:{token}");
  }

  public async Task<bool> IsLoginTokenInvalidAsync(string token)
  {
    var isValid = await IsLoginTokenValidAsync(token);
    return !isValid;
  }

  public async Task<bool> LoginTokenExistsAsync(string token)
  {
    var json = await GetCacheStringAsync($"LoginToken:{token}");
    return json is not null;
  }

  public async Task<bool> EmailConfirmationTokenExistsAsync(string token)
  {
    var json = await GetCacheStringAsync($"EmailToken:{token}");
    return json is not null;
  }

  public async Task<bool> PasswordResetTokenExistsAsync(string token)
  {
    var json = await GetCacheStringAsync($"PasswordResetToken:{token}");
    return json is not null;
  }

  public async Task<bool> RefreshTokenExistsAsync(string token)
  {
    var json = await GetCacheStringAsync($"RefreshToken:{token}");
    return json is not null;
  }

  private async Task<string?> GetCacheStringAsync(string key)
  {
    try
    {
      return await _cache.GetStringAsync(key);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to get cache string for key: {Key}", key);
      throw new CacheException("GET_STRING", key, ex);
    }
  }

  public Task<bool> IsRefreshTokenValidAsync(string token)
  {
    return GetAsync<bool>($"RefreshToken:{token}");
  }

  public Task SaveRefreshTokenAsync(string token)
  {
    return SetAsync($"RefreshToken:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
    });
  }

  public Task InvalidateRefreshTokenAsync(string token)
  {
    return SetAsync($"RefreshToken:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(8)
    });
  }
}
