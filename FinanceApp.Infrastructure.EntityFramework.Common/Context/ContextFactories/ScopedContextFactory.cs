using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Infrastructure.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Context.ContextFactories;

public class ScopedContextFactory<TContext> : IDbContextFactory<TContext>, IScopedContextFactory<TContext> where TContext : FinanceAppDbContext
{
  private readonly IDbContextFactory<TContext> _pooledFactory;
  private readonly ICurrentUserService _currentUserService;
  public ScopedContextFactory(
    IDbContextFactory<TContext> pooledFactory, ICurrentUserService currentUserService)
  {
    _pooledFactory = pooledFactory;
    _currentUserService = currentUserService;
  }

  /// <inheritdoc />
  public TContext CreateDbContext()
  {
    var context = _pooledFactory.CreateDbContext();
    context.UserName = _currentUserService.UserName;
    return context;
  }
}
