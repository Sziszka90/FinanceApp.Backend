using System.Reflection;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Mssql.Context;

public class FinanceAppMssqlDbContext : FinanceAppDbContext
{
  /// <inheritdoc />
  public FinanceAppMssqlDbContext(
    DbContextOptions<FinanceAppMssqlDbContext> options,
    ICurrentUserService? currentUserService = null) : base(options, currentUserService) { }

  protected override void OnModelCreatingProviderSpecific(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}
