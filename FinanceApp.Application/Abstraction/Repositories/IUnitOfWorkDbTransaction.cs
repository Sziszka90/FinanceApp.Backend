namespace FinanceApp.Application.Abstraction.Repositories;

public interface IUnitOfWorkDbTransaction
{
  /// <summary>Specifies the UnitOfWork associated with the transaction.</summary>
  /// <returns>The UnitOfWork associated with the transaction</returns>
  IUnitOfWork UnitOfWork { get; }

  /// <summary>
  /// Commits all changes made to the database in the current transaction asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns></returns>
  Task CommitAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Discards all changes made to the database in the current transaction asynchronously.
  /// </summary>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns></returns>
  Task RollbackAsync(CancellationToken cancellationToken = default);
}
