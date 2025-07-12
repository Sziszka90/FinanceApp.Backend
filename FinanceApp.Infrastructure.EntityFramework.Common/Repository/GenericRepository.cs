using System.Linq.Expressions;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Common;
using FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;
  private readonly FinanceAppDbContext _dbContext;

  public GenericRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider)
  {
    _filteredQueryProvider = filteredQueryProvider;
    _dbContext = dbContext;
  }

  /// <inheritdoc />
  public async Task<List<T>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await _filteredQueryProvider.Query<T>()
                            .AsNoTracking()
                            .ToListAsync(cancellationToken);
    }

    return await _filteredQueryProvider.Query<T>()
                          .ToListAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await _filteredQueryProvider.Query<T>()
                            .AsNoTracking()
                            .Where(predicate)
                            .ToListAsync(cancellationToken);
    }

    return await _filteredQueryProvider.Query<T>()
                          .Where(predicate)
                          .ToListAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<List<T>> GetQueryAsync(QueryCriteria<T> criteria, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<T>()
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
  public async Task<T?> GetByIdAsync(Guid id, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<T>();

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<T>();

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    return await query.SingleAsync(predicate, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<T>();

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    return await query.SingleOrDefaultAsync(predicate, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<T> GetFirstAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<T>();

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    return await query.FirstAsync(predicate, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<T>();

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    return await query.FirstOrDefaultAsync(predicate, cancellationToken);
  }

  /// <inheritdoc />
  public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
  {
    await _dbContext.Set<T>()
                   .AddAsync(entity, cancellationToken);
    return entity;
  }

  /// <inheritdoc />
  public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
  {
    var existingEntity = await GetByIdAsync(entity.Id, cancellationToken: cancellationToken) ?? throw new KeyNotFoundException($"The entity of type '{typeof(T).Name}' with id '{entity.Id}' was not found");

    entity.Created = existingEntity.Created;

    _dbContext.Entry(existingEntity)
             .CurrentValues
             .SetValues(entity);
    return existingEntity;
  }

  /// <inheritdoc />
  public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
  {
    _dbContext.Set<T>()
             .Remove(entity);
    return Task.CompletedTask;
  }
}
