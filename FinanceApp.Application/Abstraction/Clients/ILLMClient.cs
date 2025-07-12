using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ILLMClient
{
  /// <summary>
  /// Creates a transaction group based on the provided prompt.
  /// </summary>
  /// <param name="prompt"></param>
  /// <param name="user"></param>
  /// <param name="transactionGroups"></param>
  /// <returns>Result<List<TransactionGroup>></returns>
  public Task<Result<List<Dictionary<string, string>>>> MatchTransactionGroup(List<string> transactionNames, List<string> existingGroups, User user);
}
