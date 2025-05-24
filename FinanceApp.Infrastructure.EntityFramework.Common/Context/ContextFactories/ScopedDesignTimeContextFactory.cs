using FinanceApp.Infrastructure.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceApp.Infrastructure.EntityFramework.Context.ContextFactories;

public class ScopedDesignTimeContextFactory<TContext> : IDesignTimeDbContextFactory<TContext> where TContext : FinanceAppDbContext
{
  private readonly IDbContextFactory<TContext> _pooledFactory;

  public ScopedDesignTimeContextFactory(IDbContextFactory<TContext> pooledFactory)
  {
    _pooledFactory = pooledFactory;
  }

  public TContext CreateDbContext(string[] args)
  {
    var context = _pooledFactory.CreateDbContext();
    return context;
  }
}
