using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using Microsoft.Extensions.Caching.Distributed;

namespace FinanceApp.Backend.Infrastructure.Cache;

public class CacheManager : ICacheManager
{
  private readonly IDistributedCache _cache;

  public CacheManager(IDistributedCache cache)
  {
    _cache = cache;
  }

  public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions? options = null)
  {
    var json = JsonSerializer.Serialize(value);
    var cacheOptions = options ?? new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    };
    await _cache.SetStringAsync(key, json, cacheOptions);
  }

  public async Task<T?> GetAsync<T>(string key)
  {
    var json = await _cache.GetStringAsync(key);
    return json is null ? default : JsonSerializer.Deserialize<T>(json);
  }

  public async Task RemoveAsync(string key)
  {
    await _cache.RemoveAsync(key);
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

  public async Task InvalidateTokenAsync(string token)
  {
    await SetAsync($"Token:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    });
  }

  public async Task SaveTokenAsync(string token)
  {
    await SetAsync($"Token:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    });
  }

  public async Task<bool> IsTokenValidAsync(string token)
  {
    return await GetAsync<bool>($"Token:{token}");
  }

  public async Task<bool> IsTokenInvalidAsync(string token)
  {
    var isValid = await IsTokenValidAsync(token);
    return !isValid;
  }

  // Check if token exists in cache (regardless of validity)
  public async Task<bool> TokenExistsAsync(string token)
  {
    var json = await _cache.GetStringAsync($"Token:{token}");
    return json is not null;
  }

  public async Task<bool> EmailConfirmationTokenExistsAsync(string token)
  {
    var json = await _cache.GetStringAsync($"EmailToken:{token}");
    return json is not null;
  }

  public async Task<bool> PasswordResetTokenExistsAsync(string token)
  {
    var json = await _cache.GetStringAsync($"PasswordResetToken:{token}");
    return json is not null;
  }
}
