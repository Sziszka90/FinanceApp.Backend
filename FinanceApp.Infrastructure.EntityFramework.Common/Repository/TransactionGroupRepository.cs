using EFCore.BulkExtensions;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class TransactionGroupRepository : GenericRepository<TransactionGroup>, ITransactionGroupRepository
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;
  private readonly FinanceAppDbContext _dbContext;

  /// <inheritdoc />
  public TransactionGroupRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider
  ) : base(dbContext, filteredQueryProvider)
  {
    _filteredQueryProvider = filteredQueryProvider;
    _dbContext = dbContext;
  }

  public new async Task<TransactionGroup?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _filteredQueryProvider.Query<TransactionGroup>()
                          .FirstOrDefaultAsync(tg => tg.Id == id);
  }

  public new async Task<List<TransactionGroup>> GetAllAsync(bool noTracking = true, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await _filteredQueryProvider.Query<TransactionGroup>()
                            .AsNoTracking()
                            .Include(tg => tg.User)
                            .ToListAsync(cancellationToken);
    }

    return await _filteredQueryProvider.Query<TransactionGroup>()
                          .Include(tg => tg.User)
                          .ToListAsync(cancellationToken);
  }

  public async Task<List<TransactionGroup>> CreateTransactionGroupsAsync(List<TransactionGroup> transactionGroups, CancellationToken cancellationToken = default)
  {
    await _dbContext.BulkInsertAsync(transactionGroups, cancellationToken: cancellationToken);
    return transactionGroups;
  }

  public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    await _dbContext.TransactionGroup
        .Where(g => g.User.Id == userId)
        .ExecuteDeleteAsync(cancellationToken);
  }
}
