using System.Linq.Expressions;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Common;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface IRepository<T> where T : BaseEntity
{
  /// <summary>
  /// Get a list of all entities in the repository that satisfy a specified condition
  /// </summary>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Found results in a list</returns>
  Task<List<T>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get a list of all entities in the repository that satisfy a specified condition
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition</param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Found results in a list</returns>
  Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get a list of all entities in the repository that satisfy a specified condition
  /// </summary>
  /// <param name="criteria">QueryCriteria<T> to specify the query condition</param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Found results in a list</returns>
  Task<List<T>> GetQueryAsync(QueryCriteria<T> criteria, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets a single entity based on ID
  /// </summary>
  /// <param name="id">ID of the entity</param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>BaseEntity</returns>
  Task<T?> GetByIdAsync(Guid id, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets a single entity based on predicate
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition.</param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>BaseEntity</returns>
  Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Returns the only element in the repository that satisfies a specified condition or a default value if no such element
  /// exists.
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition.</param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>BaseEntity</returns>
  Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Returns the first element in the repository that satisfies a specified condition.
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition.</param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>BaseEntity</returns>
  Task<T> GetFirstAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Returns the first element in the repository that satisfies a specified condition or a default value if no such
  /// element is found.
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition.</param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>BaseEntity</returns>
  Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Adds the given entity to the repository. But does not save the changes to the persistence layer.
  /// </summary>
  /// <param name="entity">The entity to add.</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The created entity</returns>
  Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

  /// <summary>
  /// Updates an existing entity in the repository.
  /// </summary>
  /// <param name="entity">The entity containing the modified data</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The updated entity</returns>
  Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes an entity from the repository.
  /// </summary>
  /// <param name="entity">The entity to delete</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns></returns>
  Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes all entities from the repository.
  /// </summary>
  /// <param name="entities">The entities to delete</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns></returns>
  void DeleteAllAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);
}
