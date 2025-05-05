using System.Reflection;
using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Context;

public abstract class FinanceAppDbContext : DbContext
{
  #region Properties

  public DbSet<IncomeTransaction> IncomeTransaction => Set<IncomeTransaction>();
  public DbSet<ExpenseTransaction> ExpenseTransaction => Set<ExpenseTransaction>();
  public DbSet<Saving> Saving => Set<Saving>();
  public DbSet<Investment> Investment => Set<Investment>();
  public DbSet<Domain.Entities.User> User => Set<Domain.Entities.User>();

  #endregion

  #region Constructors

  protected FinanceAppDbContext(DbContextOptions options) : base(options) { }

  #endregion

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    OnModelCreatingProviderSpecific(modelBuilder);
  }

  /// <summary>
  /// Further configure model in derived contexts while keeping common configuration in the parent
  /// </summary>
  /// <param name="modelBuilder"></param>
  protected virtual void OnModelCreatingProviderSpecific(ModelBuilder modelBuilder)
  {
    // Enables provider-specific model creation setups while keeping Seeding in a central spot 
  }

  /// <inheritdoc />
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    // No operation, config us done over the PackagingUnitContentBaseConfiguration and its inherits.
  }
}