using System.Reflection;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Mssql.Context;

public class FinanceAppMssqlDbContext : FinanceAppDbContext
{
  #region Constructors

  /// <inheritdoc />
  public FinanceAppMssqlDbContext(
    DbContextOptions<FinanceAppMssqlDbContext> options,
    ICurrentUserService currentUserService) : base(options, currentUserService) { }

  #endregion

  protected override void OnModelCreatingProviderSpecific(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
