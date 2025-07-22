using EFCore.BulkExtensions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public class TransactionGroupRepository : GenericRepository<TransactionGroup>, ITransactionGroupRepository
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;

  /// <inheritdoc />
  public TransactionGroupRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider
  ) : base(dbContext, filteredQueryProvider)
  {
    _filteredQueryProvider = filteredQueryProvider;
  }

  public async Task<List<TransactionGroup>> BatchCreateTransactionGroupsAsync(List<TransactionGroup> transactionGroups, CancellationToken cancellationToken = default)
  {
    await _dbContext.BulkInsertAsync(transactionGroups, cancellationToken: cancellationToken);
    return transactionGroups;
  }

  public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    var query = _filteredQueryProvider.Query<TransactionGroup>().Where(x => x.User.Id == userId);

    _dbContext.TransactionGroup.RemoveRange(await query.ToListAsync(cancellationToken));
  }

  public async Task<List<TransactionGroup>> GetAllByUserIdAsync(Guid userId, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    var query = _dbContext.TransactionGroup.Include(x => x.User).Where(x => x.User.Id == userId);

    if (noTracking)
    {
      query = query.AsNoTracking();
    }

    return await query.ToListAsync(cancellationToken);
  }
}
