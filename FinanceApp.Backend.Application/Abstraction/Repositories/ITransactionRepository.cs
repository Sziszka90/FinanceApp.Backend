using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Abstraction.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
  /// <summary>
  /// Get all transactions by user ID.
  /// </summary>
  /// <param name="userId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="DatabaseException">Thrown when there is an error retrieving the transactions.</exception>
  Task<List<Transaction>> GetAllByUserIdAsync(Guid userId, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if a transaction group is used by any transaction.
  /// </summary>
  /// <param name="transactionGroupId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  /// <exception cref="DatabaseException">Thrown when there is an error checking the transaction group usage.</exception>
  Task<bool> TransactionGroupUsedAsync(Guid transactionGroupId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves all transactions by filter.
  /// </summary>
  /// <param name="transactionFilter">Optional filter for transactions.</param>
  /// <param name="noTracking">If set to true, disables EF Core tracking mechanism.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>List of existing transactions</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error retrieving the transactions.</exception>
  Task<List<Transaction>> GetAllByFilterAsync(
        TransactionFilter transactionFilter,
        bool noTracking = false,
        CancellationToken cancellationToken = default
    );

  /// <summary>
  /// Creates new transactions in bulk operation.
  /// </summary>
  /// <param name="transactions">The transactions to create.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The created transactions.</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error creating the transactions.</exception>
  Task<List<Transaction>?> BatchCreateTransactionsAsync(List<Transaction> transactions, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes all user related transactions.
  /// </summary>
  /// <param name="userId">The ID of the user whose transactions to delete.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error deleting the transactions.</exception>
  Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets transactions within a date range.
  /// </summary>
  /// <param name="startDate">Start date of the range.</param>
  /// <param name="endDate">End date of the range.</param>
  /// <param name="userId">The ID of the user whose transactions to retrieve.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>List of transactions within the specified date range.</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error retrieving the transactions.</exception>
  Task<List<Transaction>> GetTransactionsByTopTransactionGroups(DateTimeOffset startDate, DateTimeOffset endDate, Guid userId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get a transaction by its ID.
  /// </summary>
  /// <param name="id">The ID of the transaction.</param>
  /// <param name="noTracking">If set to true, disables EF Core tracking mechanism.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The transaction with the specified ID, or null if not found.</returns>
  /// <exception cref="DatabaseException">Thrown when there is an error retrieving the transaction.</exception>
  new Task<Transaction?> GetByIdAsync(Guid id, bool noTracking = false, CancellationToken cancellationToken = default);
}
