using System.Reflection;
using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Context;

public abstract class FinanceAppDbContext : DbContext
{
  public DbSet<Transaction> Transaction => Set<Transaction>();
  public DbSet<TransactionGroup> TransactionGroup => Set<TransactionGroup>();
  public DbSet<ExchangeRate> ExchangeRate => Set<ExchangeRate>();
  public DbSet<User> User => Set<User>();

  protected FinanceAppDbContext(
    DbContextOptions options) : base(options)
  {}

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
