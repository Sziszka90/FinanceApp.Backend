namespace FinanceApp.Infrastructure.Cache;

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

  public async Task InvalidateTokenAsync(string token)
  {
    await SetAsync($"Token:{token}", false, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    });
  }

  public async Task<bool> IsTokenInvalidAsync(string token)
  {
    return await GetAsync<bool>($"Token:{token}");
  }

  public async Task SaveTokenAsync(string token)
  {
    await SetAsync($"Token:{token}", true, new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    });
  }
}
