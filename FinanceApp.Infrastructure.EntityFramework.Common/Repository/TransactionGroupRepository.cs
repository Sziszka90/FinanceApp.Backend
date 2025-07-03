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

  public async Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
  {
    await _dbContext.TransactionGroup
        .Where(g => g.User.Id == userId)
        .ExecuteDeleteAsync(cancellationToken);
  }
}
