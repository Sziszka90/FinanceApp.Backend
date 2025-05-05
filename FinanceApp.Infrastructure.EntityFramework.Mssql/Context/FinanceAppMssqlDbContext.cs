using System.Reflection;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Mssql.Context;

public class FinanceAppMssqlDbContext : FinanceAppDbContext
{
  #region Constructors

  /// <inheritdoc />
  public FinanceAppMssqlDbContext(DbContextOptions<FinanceAppMssqlDbContext> options) : base(options) { }

  #endregion

  protected override void OnModelCreatingProviderSpecific(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}