using System.Data.Common;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Services;
using FinanceApp.Infrastructure.EntityFramework.Context;
using FinanceApp.Infrastructure.EntityFramework.Interceptors;
using FinanceApp.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class CustomWebApplicationFactory<TProgram>
  : WebApplicationFactory<TProgram> where TProgram : class
{
  #region Properties

  public DbConnection? SqliteDatabaseConnection { get; set; }

  #endregion

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    SqliteDatabaseConnection = new SqliteConnection("DataSource=:memory:");
    SqliteDatabaseConnection.Open();

    builder.ConfigureServices(services =>
                              {
                                services.AddSingleton(SqliteDatabaseConnection);
                                services.AddScoped<ICurrentUserService, CurrentUserService>();
                                services.AddDbContext<FinanceAppDbContext, FinanceAppSqliteDbContext>(options =>
                                                                                                      {
                                                                                                        options.UseSqlite(SqliteDatabaseConnection)
                                                                                                               .AddInterceptors(new TimestampableEntitySaveChangesInterceptor());
                                                                                                      });
                              });
    builder.UseEnvironment("Testing");
  }
}
