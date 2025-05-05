using FinanceApp.Infrastructure.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Context;

public class ScopedContextFactory<TContext> : IDbContextFactory<TContext>, IScopedContextFactory<TContext> where TContext : FinanceAppDbContext
{
  #region Members

  private readonly IDbContextFactory<TContext> _pooledFactory;

  #endregion

  #region Constructors

  public ScopedContextFactory(IDbContextFactory<TContext> pooledFactory)
  {
    _pooledFactory = pooledFactory;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public TContext CreateDbContext()
  {
    var context = _pooledFactory.CreateDbContext();
    return context;
  }

  #endregion
}