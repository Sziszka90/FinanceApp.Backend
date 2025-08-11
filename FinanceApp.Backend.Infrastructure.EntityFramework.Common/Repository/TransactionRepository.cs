using EFCore.BulkExtensions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;

  /// <inheritdoc />
  public TransactionRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider
  ) : base(dbContext, filteredQueryProvider)
  {
    _filteredQueryProvider = filteredQueryProvider;
  }

  public async Task<bool> TransactionGroupUsedAsync(Guid transactionGroupId, CancellationToken cancellationToken = default)
  {
    try
    {
      return await _filteredQueryProvider.Query<Transaction>().AnyAsync
      (t => t.TransactionGroup != null && t.TransactionGroup.Id == transactionGroupId, cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("TRANSACTION_GROUP_USED_CHECK", nameof(Transaction), transactionGroupId.ToString(), ex);
    }
  }

  public async Task<List<Transaction>> GetAllByFilterAsync(TransactionFilter transactionFilter, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _filteredQueryProvider.Query<Transaction>().Include(x => x.TransactionGroup)
        .Where(x => (transactionFilter.TransactionGroupName == null || (x.TransactionGroup != null && x.TransactionGroup.Name == transactionFilter.TransactionGroupName))
                      && (transactionFilter.TransactionName == null || x.Name == transactionFilter.TransactionName)
                      && (transactionFilter.TransactionType == null || x.TransactionType == transactionFilter.TransactionType)
                      && (transactionFilter.TransactionDate == null || x.TransactionDate.Date > transactionFilter.TransactionDate.Value.Date));

      if (transactionFilter.OrderBy is not null)
      {
        if (transactionFilter.OrderBy.Equals("TransactionGroup", StringComparison.OrdinalIgnoreCase))
        {
          query = transactionFilter.Ascending.HasValue && transactionFilter.Ascending.Value
            ? query.OrderBy(x => x.TransactionGroup != null ? x.TransactionGroup.Name : null)
            : query.OrderByDescending(x => x.TransactionGroup != null ? x.TransactionGroup.Name : null);
        }
        else if (transactionFilter.OrderBy.Equals("TransactionName", StringComparison.OrdinalIgnoreCase))
        {
          query = transactionFilter.Ascending.HasValue && transactionFilter.Ascending.Value
            ? query.OrderBy(x => x.Name)
            : query.OrderByDescending(x => x.Name);
        }
        else if (transactionFilter.OrderBy.Equals("TransactionDate", StringComparison.OrdinalIgnoreCase))
        {
          query = transactionFilter.Ascending.HasValue && transactionFilter.Ascending.Value
            ? query.OrderBy(x => x.TransactionDate)
            : query.OrderByDescending(x => x.TransactionDate);
        }
      }

      if (noTracking)
      {
        return await query.AsNoTracking().ToListAsync(cancellationToken);
      }
      return await query.ToListAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_ALL_BY_FILTER", nameof(Transaction), null, ex);
    }
  }

  public new async Task<List<Transaction>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      IQueryable<Transaction> query = _filteredQueryProvider.Query<Transaction>()
        .Include(x => x.TransactionGroup)
        .Include(x => x.User);

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.ToListAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_ALL", nameof(Transaction), null, ex);
    }
  }

  public new async Task<Transaction?> GetByIdAsync(Guid id, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      IQueryable<Transaction> query = _filteredQueryProvider.Query<Transaction>()
                            .Include(y => y.TransactionGroup);

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.FirstOrDefaultAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_BY_ID", nameof(Transaction), id.ToString(), ex);
    }
  }

  public async Task<List<Transaction>?> BatchCreateTransactionsAsync(List<Transaction> transactions, CancellationToken cancellationToken = default)
  {
    try
    {
      await _dbContext.BulkInsertAsync(transactions, cancellationToken: cancellationToken);
      return transactions;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("BATCH_CREATE", nameof(Transaction), null, ex);
    }
  }

  public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _filteredQueryProvider.Query<Transaction>().Where(x => x.User.Id == userId);

      _dbContext.Transaction.RemoveRange(await query.ToListAsync(cancellationToken));
    }
    catch (Exception ex)
    {
      throw new DatabaseException("DELETE_ALL_BY_USER_ID", nameof(Transaction), userId.ToString(), ex);
    }
  }

  public async Task<List<Transaction>> GetAllByUserIdAsync(Guid userId, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _dbContext.Transaction.Include(x => x.User).Where(x => x.User.Id == userId);

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.ToListAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_ALL_BY_USER_ID", nameof(Transaction), userId.ToString(), ex);
    }
  }

  public async Task<List<TransactionGroupAggregate>> GetTransactionGroupAggregatesAsync(
    Guid userId,
    DateTimeOffset startDate,
    DateTimeOffset endDate,
    int topCount,
    bool noTracking = false,
    CancellationToken cancellationToken = default)
  {
    try
    {
      // Use direct foreign key properties to avoid complex JOIN translation issues
      var query = _dbContext.Transaction
          .Include(t => t.TransactionGroup)
          .Where(t => t.UserId == userId &&
                      t.TransactionDate >= startDate &&
                      t.TransactionDate <= endDate &&
                      t.TransactionGroupId != null);

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      // Load data from database
      var transactions = await query.ToListAsync(cancellationToken);

      // Group and aggregate in memory
      var result = transactions
        .GroupBy(t => new { TransactionGroupId = t.TransactionGroup!.Id, Currency = t.Value.Currency })
        .Select(g => new TransactionGroupAggregate
        {
          TransactionGroup = g.First().TransactionGroup!,
          Currency = g.Key.Currency,
          TotalAmount = g.Sum(t => t.Value.Amount),
          TransactionCount = g.Count()
        })
        .OrderByDescending(g => g.TotalAmount)
        .Take(topCount)
        .ToList();

      return result;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_TRANSACTION_GROUP_AGGREGATES", nameof(Transaction), userId.ToString(), ex);
    }
  }
}
