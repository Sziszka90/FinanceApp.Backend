using FinanceApp.Backend.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Interfaces;

public interface IScopedContextFactory<out TContext> where TContext : FinanceAppDbContext
{
  public TContext CreateDbContext();
}
