using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class IncomeTransactionGroupRepository : GenericRepository<IncomeTransactionGroup>, IIncomeTransactionGroupRepository
{
  #region Constructors

  /// <inheritdoc />
  public IncomeTransactionGroupRepository(FinanceAppDbContext dbContext) : base(dbContext) { }

  #endregion
}