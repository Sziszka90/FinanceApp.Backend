using FinanceApp.Infrastructure.EntityFramework.Context;
using FinanceApp.Infrastructure.EntityFramework.Interceptors;
using FinanceApp.Infrastructure.EntityFramework.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Infrastructure.EntityFramework.Sqlite;

public static class DependencyInjection
{
  #region Methods

  public static IServiceCollection AddEntityFrameworkCoreSqlitePersistence(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddPooledDbContextFactory<FinanceAppSqliteDbContext>(options =>
                                                                  {
                                                                    options.UseSqlite(configuration.GetConnectionString(Constants.ConfigurationKeys.SqliteConnectionString))
                                                                           .AddInterceptors(new TimestampableEntitySaveChangesInterceptor());
                                                                  });

    return services;
  }

  #endregion
}
