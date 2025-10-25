using System.Data;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Domain.Common;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public sealed class UnitOfWork : IUnitOfWork
{
  private readonly FinanceAppDbContext _dbContext;

  internal IDbContextTransaction? Transaction => _dbContext?.Database?.CurrentTransaction;

  private bool _disposed;

  public UnitOfWork(FinanceAppDbContext dbContext)
  {
    _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
  }

  /// <inheritdoc />
  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    await _dbContext.SaveChangesAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<IUnitOfWorkDbTransaction> BeginTransactionAsync(IsolationLevel? isolationLevel, CancellationToken cancellationToken = default)
  {
    if (isolationLevel != null)
    {
      await _dbContext.Database.BeginTransactionAsync(isolationLevel.GetValueOrDefault(), cancellationToken);
    }
    else
    {
      await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    return new UnitOfWorkDbTransaction(this);
  }

  /// <inheritdoc />
  public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
  {
    await _dbContext.Database.CommitTransactionAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
  {
    if (_dbContext.Database.CurrentTransaction != null)
    {
      await _dbContext.Database.RollbackTransactionAsync(cancellationToken);
    }
  }

  /// <inheritdoc />
  public bool EntityAttachedToDbContext<T>(T? entity) where T : class
  {
    if (entity is not BaseEntity)
    {
      return false;
    }

    return (_dbContext.Entry(entity)
                      .State != EntityState.Detached) && (_dbContext.Entry(entity)
                                                                    .State != EntityState.Deleted);
  }

  /// <inheritdoc />
  public bool Exists<T>(T? entity) where T : class
  {
    if (entity is not BaseEntity baseEntity)
    {
      return false;
    }

    var result = _dbContext.Set<T>()
                           .Find(baseEntity.Id);
    return result != null;
  }

  /// <inheritdoc />
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  private void Dispose(bool disposing)
  {
    if (!_disposed)
    {
      if (disposing)
      {
        _dbContext.Dispose();
      }

      _disposed = true;
    }
  }
}
