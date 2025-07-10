using FinanceApp.Application.Models;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ILLMClient
{
  /// <summary>
  /// Creates a transaction group based on the provided prompt.
  /// </summary>
  /// <param name="prompt"></param>
  /// <param name="user"></param>
  /// <param name="transactionGroups"></param>
  /// <param name="cancellationToken"></param>
  /// <returns>Result<List<TransactionGroup>></returns>
  public Task<Result<List<Dictionary<string, string>>>> CreateTransactionGroup(List<string> transactionNames, List<string> existingGroups, Domain.Entities.User user, CancellationToken cancellationToken = default);
}
