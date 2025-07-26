using System.Linq.Expressions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Common;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;
  protected readonly FinanceAppDbContext _dbContext;

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
    try
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
    catch (Exception ex)
    {
      throw new DatabaseException("GET_ALL", typeof(T).Name, null, ex);
    }
  }

  /// <inheritdoc />
  public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
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
    catch (Exception ex)
    {
      throw new DatabaseException("GET_ALL_WITH_PREDICATE", typeof(T).Name, null, ex);
    }
  }

  /// <inheritdoc />
  public async Task<List<T>> GetQueryAsync(QueryCriteria<T> criteria, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
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
    catch (Exception ex)
    {
      throw new DatabaseException("GET_QUERY", typeof(T).Name, null, ex);
    }
  }

  /// <inheritdoc />
  public async Task<T?> GetByIdAsync(Guid id, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _filteredQueryProvider.Query<T>();

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_BY_ID", typeof(T).Name, id.ToString(), ex);
    }
  }

  /// <inheritdoc />
  public async Task<T> GetSingleAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _filteredQueryProvider.Query<T>();

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.SingleAsync(predicate, cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_SINGLE", typeof(T).Name, null, ex);
    }
  }

  /// <inheritdoc />
  public async Task<T?> GetSingleOrDefaultAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _filteredQueryProvider.Query<T>();

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.SingleOrDefaultAsync(predicate, cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_SINGLE_OR_DEFAULT", typeof(T).Name, null, ex);
    }
  }

  /// <inheritdoc />
  public async Task<T> GetFirstAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _filteredQueryProvider.Query<T>();

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.FirstAsync(predicate, cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_FIRST", typeof(T).Name, null, ex);
    }
  }

  /// <inheritdoc />
  public async Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _filteredQueryProvider.Query<T>();

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_FIRST_OR_DEFAULT", typeof(T).Name, null, ex);
    }
  }

  /// <inheritdoc />
  public virtual async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
  {
    try
    {
      await _dbContext.Set<T>()
                     .AddAsync(entity, cancellationToken);
      return entity;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("CREATE", typeof(T).Name, entity.Id.ToString(), ex);
    }
  }

  /// <inheritdoc />
  public virtual async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
  {
    try
    {
      var existingEntity = await GetByIdAsync(entity.Id, cancellationToken: cancellationToken) ?? throw new KeyNotFoundException($"The entity of type '{typeof(T).Name}' with id '{entity.Id}' was not found");

      entity.Created = existingEntity.Created;

      _dbContext.Entry(existingEntity)
               .CurrentValues
               .SetValues(entity);
      return existingEntity;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("UPDATE", typeof(T).Name, entity.Id.ToString(), ex);
    }
  }

  /// <inheritdoc />
  public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
  {
    try
    {
      _dbContext.Set<T>()
               .Remove(entity);
      return Task.CompletedTask;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("DELETE", typeof(T).Name, entity.Id.ToString(), ex);
    }
  }

  public void DeleteAllAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
  {
    try
    {
      _dbContext.Set<T>().RemoveRange(entities);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("DELETE_RANGE", typeof(T).Name, null, ex);
    }
  }
}
