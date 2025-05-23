using System.Reflection;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Sqlite.Context;

public class FinanceAppSqliteDbContext : FinanceAppDbContext
{
  #region Constructors

  /// <inheritdoc />
  public FinanceAppSqliteDbContext(
    DbContextOptions<FinanceAppSqliteDbContext> options,
    ICurrentUserService currentUserService) : base(options, currentUserService) { }

  #endregion

  protected override void OnModelCreatingProviderSpecific(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}