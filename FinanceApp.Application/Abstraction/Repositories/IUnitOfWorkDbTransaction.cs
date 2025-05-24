namespace FinanceApp.Application.Abstraction.Repositories;

public interface IUnitOfWorkDbTransaction
{
  #region Properties

  /// <summary>Specifies the UnitOfWork associated with the transaction.</summary>
  /// <returns>The UnitOfWork associated with the transaction.</returns>
  IUnitOfWork UnitOfWork { get; }

  #endregion

  #region Methods

  /// <summary>
  /// Commits all changes made to the database in the current transaction asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
  /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
  Task CommitAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Discards all changes made to the database in the current transaction asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
  /// <exception cref="OperationCanceledException">If the <see cref="CancellationToken" /> is canceled.</exception>
  Task RollbackAsync(CancellationToken cancellationToken = default);

  #endregion
}
