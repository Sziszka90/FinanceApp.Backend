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
  /// <returns>List<T>Found results in a list</returns>
  Task<List<T>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get a list of all entities in the repository that satisfy a specified condition
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition</param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>List<T>Found results in a list</returns>
  Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default);

  /// <summary>
  /// Get a list of all entities in the repository that satisfy a specified condition
  /// </summary>
  /// <param name="criteria">QueryCriteria<T> to specify the query condition</param>
  /// <param name="noTracking">If set to true than disables EF core tracking mechanism</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>List<T>Found results in a list</returns>
  Task<List<T>> GetQueryAsync(QueryCriteria<T> criteria, bool noTracking = true, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets a single entity based on ID
  /// </summary>
  /// <param name="id">ID of the entity</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>BaseEntity</returns>
  Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets a single entity based on predicate
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition.</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>BaseEntity</returns>
  Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

  /// <summary>
  /// Returns the only element in the repository that satisfies a specified condition or a default value if no such element
  /// exists.
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition.</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>
  /// The single element that satisfies the condition in <paramref name="predicate" />, or
  /// <see langword="default" />(<typeparamref name="T" />) if no such element is found.
  /// </returns>
  Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

  /// <summary>
  /// Returns the first element in the repository that satisfies a specified condition.
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition.</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The first element that satisfies the condition in <paramref name="predicate" />.</returns>
  Task<T> GetFirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

  /// <summary>
  /// Returns the first element in the repository that satisfies a specified condition or a default value if no such
  /// element is found.
  /// </summary>
  /// <param name="predicate">A function to test each element for a condition.</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>
  /// The first element that satisfies the condition in <paramref name="predicate" />, or <see langword="default" />
  /// (<typeparamref name="T" />) if no such element is found.
  /// </returns>
  Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

  /// <summary>
  /// Adds the given entity to the repository. But does not save the changes to the persistence layer.
  /// </summary>
  /// <param name="entity">The entity to add.</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The created entity.</returns>
  Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

  /// <summary>
  /// Updates an existing entity in the repository.
  /// </summary>
  /// <param name="entity">The entity containing the modified data</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <exception cref="KeyNotFoundException">entity does not exist</exception>
  Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes an entity from the repository by its key identifier.
  /// </summary>
  /// <remarks>
  /// Right now this method is not differentiating between "Successfully deleted entity" and "Entity didn't exist
  /// before already" cases, in both scenarios the resulting absence of the entity will be considered as a successful
  /// result.
  /// </remarks>
  /// <param name="id">The identifier of the entity to delete</param>
  /// <param name="cancellationToken">Cancellation token</param>
  Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

  /// <summary>
  /// Removes an entity from the repository.
  /// </summary>
  /// <param name="entity">The entity to delete.</param>
  Task DeleteAsync(T entity);
}
