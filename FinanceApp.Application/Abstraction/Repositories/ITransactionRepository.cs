using FinanceApp.Application.Dtos.TransactionDtos;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface ITransactionRepository : IRepository<Domain.Entities.Transaction>
{
  /// <summary>
  /// Retrieves all transactions.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<List<Domain.Entities.Transaction>> GetAllAsync(
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
  Task<List<Domain.Entities.Transaction>?> CreateMultipleTransactionsAsync(List<Domain.Entities.Transaction> transactions, CancellationToken cancellationToken = default);
}
