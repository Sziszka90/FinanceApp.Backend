using FinanceApp.Infrastructure.EntityFramework.Context;
using FinanceApp.Infrastructure.EntityFramework.Interceptors;
using FinanceApp.Infrastructure.EntityFramework.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Mssql.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Infrastructure.EntityFramework.Mssql;

public static class DependencyInjection
{
  #region Methods

  public static IServiceCollection AddEntityFrameworkCoreMssqlPersistence(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddPooledDbContextFactory<FinanceAppMssqlDbContext>(options =>
                                                                 {
                                                                   options.UseSqlServer(configuration.GetConnectionString(Constants.ConfigurationKeys.MssqlConnectionString))
                                                                          .AddInterceptors(new TimestampableEntitySaveChangesInterceptor());
                                                                 });

    services.AddScoped<IScopedContextFactory<FinanceAppDbContext>, ScopedContextFactory<FinanceAppMssqlDbContext>>();

    return services;
  }

  #endregion
}