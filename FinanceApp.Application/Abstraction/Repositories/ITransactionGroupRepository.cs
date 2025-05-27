namespace FinanceApp.Application.Abstraction.Repositories;

public interface ITransactionGroupRepository : IRepository<Domain.Entities.TransactionGroup>
{
  public Task<Domain.Entities.TransactionGroup?> GetByIdWithLimitAndIconAsync(Guid id, CancellationToken cancellationToken);
}
