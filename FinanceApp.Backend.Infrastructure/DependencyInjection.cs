using FinanceApp.Backend.Infrastructure.Cache;
using FinanceApp.Backend.Infrastructure.EntityFramework;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common;
using FinanceApp.Backend.Infrastructure.RabbitMq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Backend.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddEntityFrameworkCore(configuration);
    services.AddEntityFrameworkCorePersistence();
    services.AddRabbitMq();
    services.AddCache(configuration);

    return services;
  }

  public static IServiceCollection AddInfrastructureTesting(this IServiceCollection services)
  {
    services.AddEntityFrameworkCorePersistence();

    return services;
  }
}
