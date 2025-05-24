using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class ExpenseTransactionGroupRepository : GenericRepository<ExpenseTransactionGroup>, IExpenseTransactionGroupRepository
{
  #region Constructors

  /// <inheritdoc />
  public ExpenseTransactionGroupRepository(FinanceAppDbContext dbContext) : base(dbContext) { }

  #endregion
}
