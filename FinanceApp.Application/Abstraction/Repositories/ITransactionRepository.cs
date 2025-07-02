using FinanceApp.Application.Dtos.TransactionDtos;

namespace FinanceApp.Application.Abstraction.Repositories;

public interface ITransactionRepository : IRepository<Domain.Entities.Transaction>
{
  /// <summary>
  /// Retrieves all transactions.
  /// </summary>
  /// <param name="id"></param>
  /// <param name="cancellationToken"></param>
  /// <returns></returns>
  Task<List<Domain.Entities.Transaction>> GetAllAsync(
        TransactionFilter? transactionFilter,
        bool noTracking = false,
        CancellationToken cancellationToken = default
    );
}
