using FinanceApp.Application.Dtos.TransactionDtos;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface ITransactionRepository : IRepository<Domain.Entities.Transaction>
{
    Task<List<Domain.Entities.Transaction>> GetAllAsync(
        TransactionFilter? transactionFilter,
        bool noTracking = false,
        CancellationToken cancellationToken = default
    );
}
