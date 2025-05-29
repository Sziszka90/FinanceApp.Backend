using System.Reflection;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Mssql.Context;

public class FinanceAppMssqlDbContext : FinanceAppDbContext
{
  /// <inheritdoc />
  public FinanceAppMssqlDbContext(
    DbContextOptions<FinanceAppMssqlDbContext> options) : base(options) { }

  protected override void OnModelCreatingProviderSpecific(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
