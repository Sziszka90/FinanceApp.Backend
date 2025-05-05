using System.Reflection;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Sqlite.Context;

public class FinanceAppSqliteDbContext : FinanceAppDbContext
{
  #region Constructors

  /// <inheritdoc />
  public FinanceAppSqliteDbContext(DbContextOptions<FinanceAppSqliteDbContext> options) : base(options) { }

  #endregion

  protected override void OnModelCreatingProviderSpecific(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}