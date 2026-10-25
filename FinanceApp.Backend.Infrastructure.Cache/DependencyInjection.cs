
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace FinanceApp.Backend.Infrastructure.Cache;

public static class DependencyInjection
{
  public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddStackExchangeRedisCache(options =>
    {
      options.Configuration = configuration.GetSection("CacheSettings:ConnectionString").Value;
      options.InstanceName = configuration.GetSection("CacheSettings:InstanceName").Value;
    });
    var redisConnectionString = configuration.GetSection("CacheSettings:ConnectionString").Value;
    if (string.IsNullOrWhiteSpace(redisConnectionString))
    {
      throw new InvalidOperationException("CacheSettings:ConnectionString is required for distributed coordination.");
    }

    services.AddSingleton<IConnectionMultiplexer>(_ =>
    {
      var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
      redisOptions.AbortOnConnectFail = false;
      return ConnectionMultiplexer.Connect(redisOptions);
    });
    services.AddSingleton<IExchangeRateRefreshLock, RedisExchangeRateRefreshLock>();
    services.AddSingleton<ITokenCacheManager, TokenCacheManager>();
    services.AddSingleton<IExchangeRateCacheManager, ExchangeRateCacheManager>();
    services.AddSingleton<IPdfImportStatusStore, PdfImportStatusStore>();
    return services;
  }
}
