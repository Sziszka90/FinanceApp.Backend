using EFCore.BulkExtensions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
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
    return await _filteredQueryProvider.Query<Transaction>().AnyAsync
    (t => t.TransactionGroup != null && t.TransactionGroup.Id == transactionGroupId, cancellationToken);
  }

  public async Task<List<Transaction>> GetAllByFilterAsync(TransactionFilter transactionFilter, bool noTracking = false, CancellationToken cancellationToken = default)
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

  public new async Task<List<Transaction>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default)
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

  public new async Task<Transaction?> GetByIdAsync(Guid id, bool noTracking = false, CancellationToken cancellationToken = default)
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

  public async Task<List<Transaction>?> BatchCreateTransactionsAsync(List<Transaction> transactions, CancellationToken cancellationToken = default)
  {
    await _dbContext.BulkInsertAsync(transactions, cancellationToken: cancellationToken);
    return transactions;
  }

  public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    var query = _dbContext.Transaction.Where(x => x.User.Id == userId);

    _dbContext.Transaction.RemoveRange(await query.ToListAsync(cancellationToken));
  }

  public async Task<List<Transaction>> GetAllByUserIdAsync(Guid userId, bool noTracking = false, CancellationToken cancellationToken = default)
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

  public async Task<List<Transaction>> GetTransactionsByTopTransactionGroups(
    DateTimeOffset startDate,
    DateTimeOffset endDate,
    Guid userId,
    int? top,
    CancellationToken cancellationToken = default)
  {
    var providerName = _dbContext.Database.ProviderName ?? throw new InvalidOperationException("Database provider name is null");
    var sql = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName, top);

    object[] parameters;

    if (providerName.Contains("SqlServer"))
    {
      var paramList = new List<SqlParameter>
      {
        new SqlParameter("@userId", userId),
        new SqlParameter("@startDate", startDate),
        new SqlParameter("@endDate", endDate)
      };

      if (top.HasValue)
      {
        paramList.Add(new SqlParameter("@top", top.Value));
      }

      parameters = paramList.ToArray();
    }
    else if (providerName.Contains("Sqlite"))
    {
      var paramList = new List<SqliteParameter>
      {
        new SqliteParameter("@userId", userId),
        new SqliteParameter("@startDate", startDate),
        new SqliteParameter("@endDate", endDate)
      };

      if (top.HasValue)
      {
        paramList.Add(new SqliteParameter("@top", top.Value));
      }

      parameters = paramList.ToArray();
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
}
