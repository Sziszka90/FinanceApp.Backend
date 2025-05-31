using FinanceApp.Application.Abstraction.Repositories;
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

  public new async Task<List<Transaction>> GetAllAsync(bool noTracking = false, CancellationToken cancellationToken = default)
  {
    if (noTracking)
    {
      return await _filteredQueryProvider.Query<Transaction>()
                            .Include(y => y.TransactionGroup)
                            .AsNoTracking()
                            .ToListAsync(cancellationToken);
    }

    return await _filteredQueryProvider.Query<Transaction>()
                          .Include(y => y.TransactionGroup)
                          .ToListAsync(cancellationToken);
  }

  public new async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
  {
    return await _filteredQueryProvider.Query<Transaction>()
                          .Include(y => y.TransactionGroup)
                          .FirstOrDefaultAsync(y => y.Id == id, cancellationToken);
  }
}
