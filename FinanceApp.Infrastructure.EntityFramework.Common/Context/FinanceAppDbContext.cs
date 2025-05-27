using System.Reflection;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Domain.Common;
using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Context;

public abstract class FinanceAppDbContext : DbContext
{
  private string UserName { get; set; } = String.Empty;

  public DbSet<Transaction> Transaction => Set<Transaction>();
   public DbSet<TransactionGroup> TransactionGroup => Set<TransactionGroup>();
  public DbSet<Saving> Saving => Set<Saving>();
  public DbSet<Investment> Investment => Set<Investment>();
  public DbSet<Domain.Entities.User> User => Set<Domain.Entities.User>();

  protected FinanceAppDbContext(
    DbContextOptions options,
    ICurrentUserService? currentUserService = null) : base(options)
  {
    if (currentUserService is not null)
    {
      UserName = currentUserService.UserName;
    }
  }

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
    modelBuilder.Entity<Transaction>()
      .HasQueryFilter(x => x.User.UserName == UserName);

    modelBuilder.Entity<TransactionGroup>()
      .HasQueryFilter(x => x.User.UserName == UserName);
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
