using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface ITransactionRepository : IRepository<Domain.Entities.Transaction>
{
  /// <summary>
  /// Retrieves all transactions.
  /// </summary>
  /// <param name="transactionFilter">Optional filter for transactions.</param>
  /// <param name="noTracking">If set to true, disables EF Core tracking mechanism.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>List of existing transactions</returns>
  Task<List<Transaction>> GetAllAsync(
        TransactionFilter? transactionFilter,
        bool noTracking = false,
        CancellationToken cancellationToken = default
    );

  /// <summary>
  /// Delete all transactions associated with a specific user ID.
  /// </summary>
  /// <param name="userId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates new transactions in bulk operation.
  /// </summary>
  /// <param name="transactions">The transactions to create.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>The created transactions.</returns>
  Task<List<Transaction>?> CreateMultipleTransactionsAsync(List<Transaction> transactions, CancellationToken cancellationToken = default);
}
