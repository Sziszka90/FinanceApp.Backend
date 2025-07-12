using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface ITransactionGroupRepository : IRepository<TransactionGroup>
{
  /// <summary>
  /// Retrieves a transaction group by its ID.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken"></param>
  /// <returns>TransactionGroup</returns>
  public new Task<TransactionGroup?> GetByIdAsync(Guid id, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Delete all transaction groups associated with a specific user ID.
  /// </summary>
  /// <param name="userId"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Retrieves all transaction groups.
  /// </summary>
  /// <param name="noTracking"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>List of existing TransactionGroups</returns>
  public new Task<List<TransactionGroup>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default);


  /// <summary>
  /// Creates a list of transaction groups.
  /// </summary>
  /// <param name="transactionGroups"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>List of created TransactionGroups</returns>
  public Task<List<TransactionGroup>> CreateTransactionGroupsAsync(List<TransactionGroup> transactionGroups, CancellationToken cancellationToken = default);
}
