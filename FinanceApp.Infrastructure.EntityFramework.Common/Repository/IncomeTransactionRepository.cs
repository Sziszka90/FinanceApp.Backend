using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class IncomeTransactionRepository : GenericRepository<IncomeTransaction>, IIncomeTransactionRepository
{
  /// <inheritdoc />
  public IncomeTransactionRepository(FinanceAppDbContext dbContext) : base(dbContext) { }

  public new async Task<List<IncomeTransaction>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await DbContext.Set<IncomeTransaction>()
                            .Include(y => y.TransactionGroup)
                            .AsNoTracking()
                            .ToListAsync(cancellationToken);
    }

    return await DbContext.Set<IncomeTransaction>()
                          .Include(y => y.TransactionGroup)
                          .ToListAsync(cancellationToken);
  }

  public new async Task<IncomeTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await DbContext.Set<IncomeTransaction>()
                          .Include(y => y.TransactionGroup)
                          .FirstOrDefaultAsync(y => y.Id == id, cancellationToken);
  }
}
