using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class ExpenseTransactionGroupRepository : GenericRepository<ExpenseTransactionGroup>, IExpenseTransactionGroupRepository
{
  /// <inheritdoc />
  public ExpenseTransactionGroupRepository(FinanceAppDbContext dbContext) : base(dbContext) { }
}
