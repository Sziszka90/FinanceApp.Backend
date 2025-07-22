
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FinanceApp.Backend.Application.Abstraction.Clients;

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
    services.AddSingleton<ICacheManager, CacheManager>();
    return services;
  }
}
