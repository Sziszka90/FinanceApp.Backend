using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Infrastructure.EntityFramework.Interfaces;

public interface IScopedContextFactory<out TContext> where TContext : FinanceAppDbContext
{
  #region Methods

  public TContext CreateDbContext();

  #endregion
}