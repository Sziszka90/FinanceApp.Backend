using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context.ContextFactories;
using FinanceApp.Backend.Infrastructure.EntityFramework.Interceptors;
using FinanceApp.Backend.Infrastructure.EntityFramework.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Mssql;

public static class DependencyInjection
{
  public static IServiceCollection AddEntityFrameworkCoreMssqlPersistence(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddPooledDbContextFactory<FinanceAppMssqlDbContext>(options =>
                                                                 {
                                                                   options.UseSqlServer(configuration.GetConnectionString(Constants.ConfigurationKeys.MSSQL_CONNECTION_STRING),
                                                                                       sqlOptions =>
                                                                                       {
                                                                                         sqlOptions.EnableRetryOnFailure();
                                                                                       })
                                                                          .AddInterceptors(new TimestampableEntitySaveChangesInterceptor());
                                                                 });

    services.AddScoped<IScopedContextFactory<FinanceAppDbContext>, ScopedContextFactory<FinanceAppMssqlDbContext>>();

    return services;
  }
}
