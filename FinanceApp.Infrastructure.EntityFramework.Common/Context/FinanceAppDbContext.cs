using System.Reflection;
using System.Security.Claims;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Domain.Common;
using FinanceApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Context;

public abstract class FinanceAppDbContext : DbContext
{
  private readonly ICurrentUserService _currentUserService;

  #region Properties

  public DbSet<IncomeTransaction> IncomeTransaction => Set<IncomeTransaction>();
  public DbSet<ExpenseTransaction> ExpenseTransaction => Set<ExpenseTransaction>();
  public DbSet<Saving> Saving => Set<Saving>();
  public DbSet<Investment> Investment => Set<Investment>();
  public DbSet<Domain.Entities.User> User => Set<Domain.Entities.User>();

  #endregion

  #region Constructors

  protected FinanceAppDbContext(
    DbContextOptions options,
    ICurrentUserService currentUserService) : base(options)
  {
    _currentUserService = currentUserService;
  }

  #endregion

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    OnModelCreatingProviderSpecific(modelBuilder);
    SetupGlobalFilters(modelBuilder);
  }

  /// <summary>
  /// Setup global query filter
  /// </summary>
  private void SetupGlobalFilters(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<BaseTransaction>()
      .HasQueryFilter(x => x.User.UserName == _currentUserService.UserName);

    modelBuilder.Entity<BaseTransactionGroup>()
      .HasQueryFilter(x => x.User.UserName == _currentUserService.UserName);

  }

  /// <summary>
  /// Further configure model in derived contexts while keeping common configuration in the parent
  /// </summary>
  /// <param name="modelBuilder"></param>
  protected virtual void OnModelCreatingProviderSpecific(ModelBuilder modelBuilder)
  {
    // Enables provider-specific model creation setups while keeping Seeding in a central spot 
    // Global filter for Product
  }

  /// <inheritdoc />
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    // No operation, config us done over the PackagingUnitContentBaseConfiguration and its inherits.
  }
}
