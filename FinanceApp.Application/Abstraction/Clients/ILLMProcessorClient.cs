using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ILLMProcessorClient
{
  /// <summary>
  /// Matches transaction names with existing groups.
  /// </summary>
  /// <param name="transactionNames"></param>
  /// <param name="transactionGroups"></param>
  /// <param name="user"></param>
  /// <returns>Matched Dictionary</returns>
  Task<Result<bool>> MatchTransactionGroup(List<string> transactionNames, List<string> existingGroups, string userId);
}
