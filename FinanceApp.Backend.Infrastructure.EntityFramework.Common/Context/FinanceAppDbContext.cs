using System.Reflection;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Context;

public abstract class FinanceAppDbContext : DbContext
{
  public DbSet<Transaction> Transaction => Set<Transaction>();
  public DbSet<TransactionGroup> TransactionGroup => Set<TransactionGroup>();
  public DbSet<ExchangeRate> ExchangeRate => Set<ExchangeRate>();
  public DbSet<User> User => Set<User>();

  protected FinanceAppDbContext(
    DbContextOptions options) : base(options)
  { }

  // Parameterless constructor for testing/mocking purposes
  protected FinanceAppDbContext() : base()
  { }

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
    // Global filter for Product
  }

  /// <inheritdoc />
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    // No operation, config us done over the PackagingUnitContentBaseConfiguration and its inherits.
  }
}
