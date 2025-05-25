using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FinanceApp.Infrastructure.EntityFramework.Sqlite.Context.Factories;

public class FinanceAppDesignTimeContextFactory : IDesignTimeDbContextFactory<FinanceAppDesignTimeSqliteDbContext>
{
    public FinanceAppDesignTimeSqliteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinanceAppDesignTimeSqliteDbContext>();

        // Adjust this to match your project setup and file location
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString(Constants.ConfigurationKeys.SqliteConnectionString);

        optionsBuilder.UseSqlite(connectionString);

        return new FinanceAppDesignTimeSqliteDbContext(optionsBuilder.Options);
    }
}
