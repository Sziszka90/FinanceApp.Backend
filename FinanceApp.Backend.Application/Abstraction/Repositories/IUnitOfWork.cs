using System.Data;

namespace FinanceApp.Backend.Application.Abstraction.Repositories;

public interface IUnitOfWork : IDisposable
{
  /// <summary>
  /// Saves all changes made in this unit of work to the database.
  /// </summary>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns></returns>
  /// <exception cref="DatabaseException">Thrown when there is an error saving changes.</exception>
  Task SaveChangesAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Opens a new database transaction
  /// </summary>
  /// <param name="isolationLevel">Transaction <see cref="IsolationLevel" /></param>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns>Created Transaction</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error starting a transaction.</exception>
  Task<IUnitOfWorkDbTransaction> BeginTransactionAsync(IsolationLevel? isolationLevel, CancellationToken cancellationToken = default);

  /// <summary>
  /// Commits the database transaction
  /// </summary>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns></returns>
  /// <exception cref="DatabaseException">Thrown when there is an error committing the transaction.</exception>
  Task CommitTransactionAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Rolls back the database transaction
  /// </summary>
  /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
  /// <returns></returns>
  /// <exception cref="DatabaseException">Thrown when there is an error rolling back the transaction.</exception>
  Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Entity is attached to db context
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="entity"></param>
  /// <returns>Boolean - success or failure</returns>
  bool EntityAttachedToDbContext<T>(T? entity) where T : class;

  /// <summary>
  /// Returns if the entity exists in the database
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="entity"></param>
  /// <returns>Boolean - success or failure</returns>
  bool Exists<T>(T? entity) where T : class;
}
