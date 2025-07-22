using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context.ContextFactories;
using FinanceApp.Backend.Infrastructure.EntityFramework.Interceptors;
using FinanceApp.Backend.Infrastructure.EntityFramework.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite;

public static class DependencyInjection
{
  public static IServiceCollection AddEntityFrameworkCoreSqlitePersistence(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddScoped<IScopedContextFactory<FinanceAppDbContext>, ScopedContextFactory<FinanceAppSqliteDbContext>>();

    services.AddPooledDbContextFactory<FinanceAppSqliteDbContext>(
    options =>
    {
      options.UseSqlite(configuration.GetConnectionString(Constants.ConfigurationKeys.SQLITE_CONNECTION_STRING))
              .AddInterceptors(new TimestampableEntitySaveChangesInterceptor());
    });

    services.AddScoped<IScopedContextFactory<FinanceAppDbContext>, ScopedContextFactory<FinanceAppSqliteDbContext>>();

    return services;
  }
}
