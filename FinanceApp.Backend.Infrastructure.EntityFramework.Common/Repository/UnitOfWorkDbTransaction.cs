using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Exceptions;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public class UnitOfWorkDbTransaction : IUnitOfWorkDbTransaction
{
  private bool _disposed;

  /// <inheritdoc />
  public IUnitOfWork UnitOfWork { get; private set; }

  public UnitOfWorkDbTransaction(IUnitOfWork unitOfWork)
  {
    UnitOfWork = unitOfWork;
  }

  /// <inheritdoc />
  public async Task CommitAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await UnitOfWork.CommitTransactionAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("TRANSACTION_COMMIT", "UnitOfWorkDbTransaction", null, ex);
    }
  }

  /// <inheritdoc />
  public async Task RollbackAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await UnitOfWork.RollbackTransactionAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("TRANSACTION_ROLLBACK", "UnitOfWorkDbTransaction", null, ex);
    }
  }

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    if (!_disposed)
    {
      try
      {
        await RollbackAsync();
        if (UnitOfWork is UnitOfWork { Transaction: not null } uow)
        {
          await uow.Transaction.DisposeAsync();
        }

        UnitOfWork = null!;
        _disposed = true;
      }
      catch (Exception)
      {
        _disposed = true;
      }
    }
  }

  /// <inheritdoc />
  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (disposing)
    {
      try
      {
        RollbackAsync()
          .GetAwaiter()
          .GetResult();
        (UnitOfWork as UnitOfWork)?.Transaction?.Dispose();

        UnitOfWork = null!;
        _disposed = true;
      }
      catch (Exception)
      {
        // Log or handle other exceptions during disposal, but don't throw
        _disposed = true;
      }
    }
  }
}
