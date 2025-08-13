using FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.ContextFactory;

public class FinanceAppMssqlDbContextFactory : IDesignTimeDbContextFactory<FinanceAppMssqlDbContext>
{
    public FinanceAppMssqlDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.Database.json", optional: false)
        .Build();

        var optionsBuilder = new DbContextOptionsBuilder<FinanceAppMssqlDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString(Constants.ConfigurationKeys.MSSQL_PROVIDER),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure();
            });

        return new FinanceAppMssqlDbContext(optionsBuilder.Options);
    }
}
