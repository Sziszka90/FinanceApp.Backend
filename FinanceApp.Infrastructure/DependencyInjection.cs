using FinanceApp.Infrastructure.EntityFramework;
using FinanceApp.Infrastructure.EntityFramework.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Infrastructure;

public static class DependencyInjection
{
  public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddEntityFrameworkCore(configuration);

    services.AddEntityFrameworkCorePersistence();

    return services;
  }

  public static IServiceCollection AddInfrastructureTesting(this IServiceCollection services)
  {
    services.AddEntityFrameworkCorePersistence();

    return services;
  }
}
