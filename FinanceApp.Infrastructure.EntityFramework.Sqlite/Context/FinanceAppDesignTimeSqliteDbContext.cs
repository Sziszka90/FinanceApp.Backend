using System.Reflection;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Sqlite.Context;

public class FinanceAppDesignTimeSqliteDbContext : FinanceAppDesignTimeDbContext
{
  /// <inheritdoc />
  public FinanceAppDesignTimeSqliteDbContext(
    DbContextOptions<FinanceAppDesignTimeSqliteDbContext> options)
    : base(options) { }

  protected override void OnModelCreatingProviderSpecific(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
