using System.Data.Common;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using FinanceApp.Backend.Infrastructure.EntityFramework.Interceptors;
using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FinanceApp.Backend.Application.BackgroundJobs.ExchangeRate;
using FinanceApp.Backend.Application.BackgroundJobs.RabbitMQ;

namespace FinanceApp.Backend.Testing.Base;

public class CustomWebApplicationFactory<TProgram>
  : WebApplicationFactory<TProgram> where TProgram : class
{
  public DbConnection? SqliteDatabaseConnection { get; set; }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    SqliteDatabaseConnection = new SqliteConnection("DataSource=:memory:");
    SqliteDatabaseConnection.Open();

    builder.ConfigureServices(services =>
    {
      services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, MockJwtAuthHandler>("Test", options => { });

      services.AddAuthorization(options =>
      {
        options.DefaultPolicy = new AuthorizationPolicyBuilder("Test")
          .RequireAuthenticatedUser()
          .Build();
      });
      services.RegisterEmailServiceMock();
      services.AddSingleton(SqliteDatabaseConnection);
      services.AddDbContext<FinanceAppDbContext, FinanceAppSqliteDbContext>(
        options =>
        {
          options.UseSqlite(SqliteDatabaseConnection)
                  .AddInterceptors(new TimestampableEntitySaveChangesInterceptor());
        });
      services.RegisterCacheManagerMock();
      services.RegisterBcryptMock();
      services.RegisterJwtMock();
      RemoveServices(new List<Dictionary<Type, Type>>
      {
        new Dictionary<Type, Type>
        {
          { typeof(IHostedService), typeof(ExchangeRateBackgroundJob) }
        },
        new Dictionary<Type, Type>
        {
          { typeof(IHostedService), typeof(RabbitMqConsumerServiceBackgroundJob) }
        }
      }, services);

    });
    builder.UseEnvironment("Testing");
  }

  private void RemoveServices(List<Dictionary<Type, Type>> serviceTypes, IServiceCollection services)
  {
    foreach (var serviceType in serviceTypes)
    {
      var descriptor = services.FirstOrDefault(d => d.ServiceType == serviceType.First().Key && d.ImplementationType == serviceType.First().Value);
      if (descriptor != null)
      {
        services.Remove(descriptor);
      }
    }
  }
}
