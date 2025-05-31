using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class InvestmentRepository : GenericRepository<Investment>, IInvestmentRepository
{
  /// <inheritdoc />
  public InvestmentRepository(
    FinanceAppDbContext dbContext,
    IFilteredQueryProvider filteredQueryProvider) : base(dbContext, filteredQueryProvider) { }
}
