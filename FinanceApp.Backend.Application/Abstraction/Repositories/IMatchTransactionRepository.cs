using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Abstraction.Repositories;

public interface IMatchTransactionRepository : IRepository<MatchTransaction>
{
  /// <summary>
  /// Get All Matched Transactions by Correlation ID
  /// </summary>
  /// <param name="correlationId">The correlation ID to filter by</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>A list of matched transactions</returns>
  Task<List<MatchTransaction>> GetAllByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);

  /// <summary>
  /// Delete All Matched Transactions by Correlation ID
  /// </summary>
  /// <param name="correlationId">The correlation ID to filter by</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>A task representing the asynchronous operation</returns>
  Task DeleteAllByCorrelationIdAsync(string correlationId, CancellationToken cancellationToken = default);
}
