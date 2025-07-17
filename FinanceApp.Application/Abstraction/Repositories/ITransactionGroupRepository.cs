using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface ITransactionGroupRepository : IRepository<TransactionGroup>
{

  /// <summary>
  /// Get all transaction groups by user ID.
  /// </summary>
  /// <param name="userId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<List<TransactionGroup>> GetAllByUserIdAsync(Guid userId, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Creates a list of transaction groups.
  /// </summary>
  /// <param name="transactionGroups"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>List of created TransactionGroups</returns>
  Task<List<TransactionGroup>> BatchCreateTransactionGroupsAsync(List<TransactionGroup> transactionGroups, CancellationToken cancellationToken = default);

  /// <summary>
  /// Deletes all transaction groups related to a specific user.
  /// </summary>
  /// <param name="userId">The ID of the user whose transaction groups to delete.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns>A task representing the asynchronous operation.</returns>
  Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
