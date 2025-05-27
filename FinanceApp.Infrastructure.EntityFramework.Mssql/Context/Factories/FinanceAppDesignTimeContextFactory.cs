using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FinanceApp.Infrastructure.EntityFramework.Mssql.Context.Factories;

public class FinanceAppDesignTimeContextFactory : IDesignTimeDbContextFactory<FinanceAppMssqlDbContext>
{
  public FinanceAppMssqlDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<FinanceAppMssqlDbContext>();

    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    var connectionString = configuration.GetConnectionString(Constants.ConfigurationKeys.MssqlConnectionString);

    optionsBuilder.UseSqlServer(connectionString);

    return new FinanceAppMssqlDbContext(optionsBuilder.Options);
  }
}
