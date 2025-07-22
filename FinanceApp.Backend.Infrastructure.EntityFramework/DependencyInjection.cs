using FinanceApp.Backend.Infrastructure.EntityFramework.Common;
using FinanceApp.Backend.Infrastructure.EntityFramework.Mssql;
using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Backend.Infrastructure.EntityFramework;

public static class DependencyInjection
{
  public static IServiceCollection AddEntityFrameworkCore(this IServiceCollection services, IConfiguration configuration)
  {
    switch (configuration.GetValue<string>(Constants.ConfigurationKeys.DATABASE_PROVIDER)?.ToLowerInvariant())
    {
      case "mssql":
        services.AddEntityFrameworkCoreMssqlPersistence(configuration);
        break;

      case "sqlite":
        services.AddEntityFrameworkCoreSqlitePersistence(configuration);
        break;

      default:
        throw new NotSupportedException("The current database provider configuration is not supported");
    }

    services.AddDatabaseContext();

    return services;
  }
}
