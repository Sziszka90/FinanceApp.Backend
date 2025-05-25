using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Domain.Entities;
using FinanceApp.Infrastructure.EntityFramework.Context;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Repository;

public class TransactionGroupRepository : GenericRepository<TransactionGroup>, ITransactionGroupRepository
{
  /// <inheritdoc />
  public TransactionGroupRepository(FinanceAppDbContext dbContext) : base(dbContext) { }
}
