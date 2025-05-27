using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FinanceApp.Infrastructure.EntityFramework.Sqlite.Context.Factories;

public class FinanceAppDesignTimeContextFactory : IDesignTimeDbContextFactory<FinanceAppSqliteDbContext>
{
    public FinanceAppSqliteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinanceAppSqliteDbContext>();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString(Constants.ConfigurationKeys.SqliteConnectionString);

        optionsBuilder.UseSqlite(connectionString);

        return new FinanceAppSqliteDbContext(optionsBuilder.Options);
    }
}
