
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Context;

public class FinanceAppDesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinanceAppDbContext>
{
    public FinanceAppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinanceAppDbContext>();

        // Load from your appsettings or just hardcode for migrations
        optionsBuilder.UseSqlServer("YourConnectionString");

        // Use a dummy user service (since there's no HTTP context during migrations)
        var dummyUserService = new DummyCurrentUserService();

        return new AppDbContext(optionsBuilder.Options, dummyUserService);
    }