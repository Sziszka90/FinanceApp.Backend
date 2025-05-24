using FinanceApp.Application.Abstraction.Repositories;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class UnitOfWorkDbTransaction : IUnitOfWorkDbTransaction
{
  #region Members

  private bool _disposed;

  #endregion

  #region Properties

  /// <inheritdoc />
  public IUnitOfWork UnitOfWork { get; private set; }

  #endregion

  #region Constructors

  public UnitOfWorkDbTransaction(IUnitOfWork unitOfWork)
  {
    UnitOfWork = unitOfWork;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  /// <remarks>
  /// See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and
  /// examples.
  /// </remarks>
  public async Task CommitAsync(CancellationToken cancellationToken = default)
  {
    await UnitOfWork.CommitTransactionAsync(cancellationToken);
  }

  /// <inheritdoc />
  /// ///
  /// <remarks>
  /// See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and
  /// examples.
  /// </remarks>
  public async Task RollbackAsync(CancellationToken cancellationToken = default)
  {
    await UnitOfWork.RollbackTransactionAsync(cancellationToken);
  }

  #region IAsyncDisposable implementation

  /// <inheritdoc />
  public async ValueTask DisposeAsync()
  {
    if (!_disposed)
    {
      await RollbackAsync();
      if (UnitOfWork is UnitOfWork { Transaction: not null } uow)
      {
        await uow.Transaction.DisposeAsync();
      }

      UnitOfWork = null!;
      _disposed = true;
    }
  }

  #endregion // IAsyncDisposable implementation

  #endregion

  #region IDisposable implementation

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
      RollbackAsync()
        .GetAwaiter()
        .GetResult();
      (UnitOfWork as UnitOfWork)?.Transaction?.Dispose();

      UnitOfWork = null!;
      _disposed = true;
    }
  }

  #endregion
}
