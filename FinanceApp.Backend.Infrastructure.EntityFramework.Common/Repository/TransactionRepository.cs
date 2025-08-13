using EFCore.BulkExtensions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.Data.SqlClient;
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
      // Use raw SQL with proper parameterization for aggregation at database level
      var sql = @"
        SELECT
          tg.Id as TransactionGroupId,
          tg.Name,
          tg.Description,
          tg.GroupIcon,
          t.Currency,
          SUM(t.Amount) as TotalAmount,
          COUNT(*) as TransactionCount
        FROM ""Transaction"" t
        INNER JOIN ""TransactionGroup"" tg ON t.TransactionGroupId = tg.Id
        WHERE t.UserId = @userId AND t.TransactionGroupId IS NOT NULL
        AND t.TransactionDate >= @startDate AND t.TransactionDate <= @endDate
        GROUP BY tg.Id, t.Currency
        ORDER BY SUM(t.Amount) DESC
        LIMIT @topCount";

      using var command = _dbContext.Database.GetDbConnection().CreateCommand();
      command.CommandText = sql;

      // Use database-agnostic parameter creation
      var userIdParam = command.CreateParameter();
      userIdParam.ParameterName = "@userId";
      userIdParam.Value = userId;
      command.Parameters.Add(userIdParam);

      var startDateParam = command.CreateParameter();
      startDateParam.ParameterName = "@startDate";
      startDateParam.Value = startDate;
      command.Parameters.Add(startDateParam);

      var endDateParam = command.CreateParameter();
      endDateParam.ParameterName = "@endDate";
      endDateParam.Value = endDate;
      command.Parameters.Add(endDateParam);

      var topCountParam = command.CreateParameter();
      topCountParam.ParameterName = "@topCount";
      topCountParam.Value = topCount;
      command.Parameters.Add(topCountParam);

      var result = new List<TransactionGroupAggregate>();
      using var reader = await command.ExecuteReaderAsync(cancellationToken);

      while (await reader.ReadAsync(cancellationToken))
      {
        var transactionGroupId = reader.GetGuid(0); // TransactionGroupId
        var name = reader.GetString(1); // Name
        var description = reader.IsDBNull(2) ? null : reader.GetString(2); // Description
        var groupIcon = reader.IsDBNull(3) ? null : reader.GetString(3); // GroupIcon
        var currency = (CurrencyEnum)reader.GetInt32(4); // Currency
        var totalAmount = reader.GetDecimal(5); // TotalAmount
        var transactionCount = reader.GetInt32(6); // TransactionCount

        var transactionGroup = new TransactionGroup(name, description, groupIcon, null!) { Id = transactionGroupId };

        result.Add(new TransactionGroupAggregate
        {
          TransactionGroup = transactionGroup,
          Currency = currency,
          TotalAmount = totalAmount,
          TransactionCount = transactionCount
        });
      }

      return result;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_TRANSACTION_GROUP_AGGREGATES", nameof(Transaction), userId.ToString(), ex);
    }
  }
}
