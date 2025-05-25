using System.Linq.Expressions;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Common;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
  protected FinanceAppDbContext DbContext { get; }

  public GenericRepository(FinanceAppDbContext dbContext)
  {
    DbContext = dbContext;
  }

  /// <inheritdoc />
  public async Task<List<T>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await DbContext.Set<T>()
                            .AsNoTracking()
                            .ToListAsync(cancellationToken);
    }

    return await DbContext.Set<T>()
                          .ToListAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await DbContext.Set<T>()
                            .AsNoTracking()
                            .Where(predicate)
                            .ToListAsync(cancellationToken);
    }

    return await DbContext.Set<T>()
                          .Where(predicate)
                          .ToListAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<List<T>> GetQueryAsync(QueryCriteria<T> criteria, bool noTracking = true, CancellationToken cancellationToken = default)
  {
    var query = DbContext.Set<T>()
                         .AsQueryable();

    var includes = criteria.Includes;
    if (includes is not null)
    {
      query = includes.Aggregate(query, (current, include) => current.Include(include));
    }

    var includesWithStrings = criteria.IncludesWithPropertyPath;
    if (includesWithStrings is not null)
    {
      query = includesWithStrings.Aggregate(query, (current, include) => current.Include(include));
    }

    var wheres = criteria.Wheres;
    if (wheres is not null)
    {
      query = wheres.Aggregate(query, (current, where) => current.Where(where));
    }

    var orderBy = criteria.OrderBy;
    if (orderBy is not null)
    {
      query = orderBy(query);
    }

    var list = noTracking
      ? await query.AsNoTracking()
                   .ToListAsync(cancellationToken)
      : await query.ToListAsync(cancellationToken);

    return list;
  }

  /// <inheritdoc />
  public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await DbContext.Set<T>()
                          .FindAsync(id);
  }

  /// <inheritdoc />
  public async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
  {
    return await DbContext.Set<T>()
                          .SingleAsync(predicate, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
  {
    return await DbContext.Set<T>()
                          .SingleOrDefaultAsync(predicate, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<T> GetFirstAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
  {
    return await DbContext.Set<T>()
                          .FirstAsync(predicate, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
  {
    return await DbContext.Set<T>()
                          .FirstOrDefaultAsync(predicate, cancellationToken);
  }

  /// <inheritdoc />
  public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
  {
    await DbContext.Set<T>()
                   .AddAsync(entity, cancellationToken);
    return entity;
  }

  /// <inheritdoc />
  public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
  {
    var existingEntity = await GetByIdAsync(entity.Id, cancellationToken) ?? throw new KeyNotFoundException($"The entity of type '{typeof(T).Name}' with id '{entity.Id}' was not found");

    entity.Created = existingEntity.Created;

    DbContext.Entry(existingEntity)
             .CurrentValues
             .SetValues(entity);
    return existingEntity;
  }

  /// <inheritdoc />
  public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var existingEntity = await GetByIdAsync(id, cancellationToken) ?? throw new KeyNotFoundException($"The entity of type '{typeof(T).Name}' with id '{id}' was not found");
    ;
    await DeleteAsync(existingEntity);
  }

  /// <inheritdoc />
  public virtual Task DeleteAsync(T entity)
  {
    DbContext.Set<T>()
             .Remove(entity);
    return Task.CompletedTask;
  }
}
