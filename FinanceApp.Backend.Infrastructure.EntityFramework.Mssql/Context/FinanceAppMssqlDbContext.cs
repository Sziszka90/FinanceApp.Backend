using System.Reflection;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Mssql.Context;

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
