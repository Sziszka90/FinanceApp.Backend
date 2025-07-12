using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface ITransactionRepository : IRepository<Transaction>
{
  /// <summary>
  /// Retrieves all transactions by filter.
  /// </summary>
  /// <param name="transactionFilter">Optional filter for transactions.</param>
  /// <param name="noTracking">If set to true, disables EF Core tracking mechanism.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>List of existing transactions</returns>
  Task<List<Transaction>> GetAllByFilterAsync(
        TransactionFilter? transactionFilter,
        bool noTracking = false,
        CancellationToken cancellationToken = default
    );

  /// <summary>
  /// Creates new transactions in bulk operation.
  /// </summary>
  /// <param name="transactions">The transactions to create.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The created transactions.</returns>
  Task<List<Transaction>?> CreateMultipleTransactionsAsync(List<Transaction> transactions, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes all user related transactions.
  /// </summary>
  /// <param name="userId">The ID of the user whose transactions to delete.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
