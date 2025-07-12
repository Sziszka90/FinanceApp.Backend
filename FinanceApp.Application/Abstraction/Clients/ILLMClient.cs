using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ILLMClient
{
  /// <summary>
  /// Matches transaction names with existing groups.
  /// </summary>
  /// <param name="transactionNames"></param>
  /// <param name="transactionGroups"></param>
  /// <param name="user"></param>
  /// <returns>Matched Dictionary</returns>
  public Task<Result<List<Dictionary<string, string>>>> MatchTransactionGroup(List<string> transactionNames, List<string> existingGroups, User user);
}
