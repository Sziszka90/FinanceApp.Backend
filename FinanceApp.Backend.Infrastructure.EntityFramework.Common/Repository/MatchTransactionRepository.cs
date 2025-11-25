using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;

public class MatchTransactionRepository : GenericRepository<MatchTransaction>, IMatchTransactionRepository
{
  public MatchTransactionRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider
  ) : base(dbContext, filteredQueryProvider)
  {

  }
}
