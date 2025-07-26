using EFCore.BulkExtensions;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Exceptions;
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
    try
    {
      await _dbContext.BulkInsertAsync(transactionGroups, cancellationToken: cancellationToken);
      return transactionGroups;
    }
    catch (Exception ex)
    {
      throw new DatabaseException("BATCH_CREATE", nameof(TransactionGroup), null, ex);
    }
  }

  public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _filteredQueryProvider.Query<TransactionGroup>().Where(x => x.User.Id == userId);

      _dbContext.TransactionGroup.RemoveRange(await query.ToListAsync(cancellationToken));
    }
    catch (Exception ex)
    {
      throw new DatabaseException("DELETE_ALL_BY_USER_ID", nameof(TransactionGroup), userId.ToString(), ex);
    }
  }

  public async Task<List<TransactionGroup>> GetAllByUserIdAsync(Guid userId, bool noTracking = false, CancellationToken cancellationToken = default)
  {
    try
    {
      var query = _dbContext.TransactionGroup.Include(x => x.User).Where(x => x.User.Id == userId);

      if (noTracking)
      {
        query = query.AsNoTracking();
      }

      return await query.ToListAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      throw new DatabaseException("GET_ALL_BY_USER_ID", nameof(TransactionGroup), userId.ToString(), ex);
    }
  }
}
