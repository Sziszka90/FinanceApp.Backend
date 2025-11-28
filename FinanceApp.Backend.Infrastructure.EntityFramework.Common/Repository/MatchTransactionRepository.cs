using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public class MatchTransactionRepository : GenericRepository<MatchTransaction>, IMatchTransactionRepository
{
  private readonly IFilteredQueryProvider _filteredQueryProvider;

  public MatchTransactionRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider
  ) : base(dbContext, filteredQueryProvider)
  {
    _filteredQueryProvider = filteredQueryProvider;
  }

  public async Task<List<MatchTransaction>> GetAllByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
  {
    return await _filteredQueryProvider.Query<MatchTransaction>()
      .Where(m => m.CorrelationId == correlationId)
      .ToListAsync(cancellationToken);
  }

  public async Task DeleteAllByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default)
  {
    var matchTransactions = await _filteredQueryProvider.Query<MatchTransaction>()
      .Where(m => m.CorrelationId == correlationId)
      .ToListAsync(cancellationToken);

    if (matchTransactions.Count != 0)
    {
      _dbContext.Set<MatchTransaction>().RemoveRange(matchTransactions);
    }
  }
}
