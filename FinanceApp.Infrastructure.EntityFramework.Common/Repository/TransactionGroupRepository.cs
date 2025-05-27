using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class TransactionGroupRepository : GenericRepository<TransactionGroup>, ITransactionGroupRepository
{
  /// <inheritdoc />
  public TransactionGroupRepository(FinanceAppDbContext dbContext) : base(dbContext) { }

  public async Task<TransactionGroup?> GetByIdWithLimitAndIconAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await DbContext.Set<TransactionGroup>()
                          .Include(tg => tg.Limit)
                          .Include(tg => tg.GroupIcon)
                          .FirstOrDefaultAsync(tg => tg.Id == id);
  }
}
