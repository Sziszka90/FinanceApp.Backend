using EFCore.BulkExtensions;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;
  private readonly FinanceAppDbContext _dbContext;

  /// <inheritdoc />
  public TransactionRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider
  ) : base(dbContext, filteredQueryProvider)
  {
    _dbContext = dbContext;
    _filteredQueryProvider = filteredQueryProvider;
  }

  public async Task<List<Transaction>> GetAllByFilterAsync(TransactionFilter transactionFilter, bool noTracking = false, CancellationToken cancellationToken = default)
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

  public new async Task<Transaction?> GetByIdAsync(Guid id, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<Transaction>()
                          .Include(y => y.TransactionGroup)
                          .Where(y => y.Id == id);

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
    var query = _filteredQueryProvider.Query<Transaction>().Where(x => x.User.Id == userId);

    _dbContext.Transaction.RemoveRange(await query.ToListAsync(cancellationToken));
  }
}
