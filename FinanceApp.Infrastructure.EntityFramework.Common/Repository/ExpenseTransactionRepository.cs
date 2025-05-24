using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class ExpenseTransactionRepository : GenericRepository<ExpenseTransaction>, IExpenseTransactionRepository
{
  #region Constructors

  /// <inheritdoc />
  public ExpenseTransactionRepository(FinanceAppDbContext dbContext) : base(dbContext) { }

  #endregion

  #region Methods

  public new async Task<List<ExpenseTransaction>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await DbContext.Set<ExpenseTransaction>()
                            .Include(y => y.TransactionGroup)
                            .AsNoTracking()
                            .ToListAsync(cancellationToken);
    }

    return await DbContext.Set<ExpenseTransaction>()
                          .Include(y => y.TransactionGroup)
                          .ToListAsync(cancellationToken);
  }

  public new async Task<ExpenseTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await DbContext.Set<ExpenseTransaction>()
                          .Include(y => y.TransactionGroup)
                          .FirstOrDefaultAsync(y => y.Id == id, cancellationToken);
  }

  #endregion
}
