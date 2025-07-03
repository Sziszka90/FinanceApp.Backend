using FinanceApp.Infrastructure.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Context.ContextFactories;

public class ScopedContextFactory<TContext> : IDbContextFactory<TContext>, IScopedContextFactory<TContext> where TContext : FinanceAppDbContext
{
  private readonly IDbContextFactory<TContext> _pooledFactory;

  public ScopedContextFactory(
    IDbContextFactory<TContext> pooledFactory)
  {
    _pooledFactory = pooledFactory;
  }

  /// <inheritdoc />
  public TContext CreateDbContext()
  {
    var context = _pooledFactory.CreateDbContext();
    return context;
  }
}
