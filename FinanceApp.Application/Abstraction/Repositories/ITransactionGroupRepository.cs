namespace FinanceApp.Application.Abstraction.Repositories;

public interface ITransactionGroupRepository : IRepository<Domain.Entities.TransactionGroup>
{
  /// <summary>
  /// Retrieves a transaction group by its ID.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>TransactionGroup</returns>
  public new Task<Domain.Entities.TransactionGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

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
  /// <returns></returns>
  public new Task<List<Domain.Entities.TransactionGroup>> GetAllAsync(bool noTracking = true, CancellationToken cancellationToken = default);


  /// <summary>
  /// Creates a list of transaction groups.
  /// </summary>
  /// <param name="transactionGroups"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  public Task<List<Domain.Entities.TransactionGroup>> CreateTransactionGroupsAsync(List<Domain.Entities.TransactionGroup> transactionGroups, CancellationToken cancellationToken = default);
}
