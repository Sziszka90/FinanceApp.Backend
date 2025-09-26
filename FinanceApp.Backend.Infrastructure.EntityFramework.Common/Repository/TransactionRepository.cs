using EFCore.BulkExtensions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;
  private readonly ISqlQueryBuilder _sqlQueryBuilder;

  /// <inheritdoc />
  public TransactionRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider,
    ISqlQueryBuilder sqlQueryBuilder
  ) : base(dbContext, filteredQueryProvider)
  {
    _filteredQueryProvider = filteredQueryProvider;
    _sqlQueryBuilder = sqlQueryBuilder;
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
      var query = _filteredQueryProvider.Query<Transaction>()
      .Include(x => x.TransactionGroup)
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
                            .Where(x => x.Id == id)
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
      var query = _dbContext.Transaction.Where(x => x.User.Id == userId);

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
      var query = _dbContext.Transaction
      .Include(x => x.User)
      .Where(x => x.User.Id == userId);

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

  public async Task<List<Transaction>> GetTransactionsByTopTransactionGroups(DateTimeOffset startDate, DateTimeOffset endDate, Guid userId, CancellationToken cancellationToken = default)
  {
    try
    {
      var providerName = _dbContext.Database.ProviderName ?? throw new InvalidOperationException("Database provider name is null");
      var sql = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName);

      object[] parameters;

      if (providerName.Contains("SqlServer"))
      {
        parameters = new[]
        {
          new SqlParameter("@userId", userId),
          new SqlParameter("@startDate", startDate),
          new SqlParameter("@endDate", endDate)
        };
      }
      else if (providerName.Contains("Sqlite"))
      {
        parameters = new[]
        {
          new SqliteParameter("@userId", userId),
          new SqliteParameter("@startDate", startDate),
          new SqliteParameter("@endDate", endDate)
        };
      }
      else
      {
        throw new NotSupportedException($"Unsupported provider: {providerName}");
      }

      var transactions = await _dbContext.Transaction
        .FromSqlRaw(sql, parameters)
        .Include(t => t.TransactionGroup)
        .ToListAsync(cancellationToken);

      return transactions;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_TRANSACTIONS_BY_TOP_TRANSACTION_GROUPS", nameof(Transaction), null, ex);
    }
  }
}
