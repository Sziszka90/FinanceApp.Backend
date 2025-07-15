using System.Data.Common;
using FinanceApp.Infrastructure.EntityFramework.Context;
using FinanceApp.Infrastructure.EntityFramework.Interceptors;
using FinanceApp.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FinanceApp.Application.BackgroundJobs.ExchangeRate;

namespace FinanceApp.Testing.Base;

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
      services.RegisterBcryptMock();
      services.RegisterJwtMock();
      services.Remove(typeof(BackgroundService), typeof(ExchangeRateBackgroundJob));
    });
    builder.UseEnvironment("Testing");
  }
}
