using FinanceApp.Application.Models;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ILLMProcessorClient
{
  /// <summary>
  /// Matches transaction names with existing groups.
  /// </summary>
  /// <param name="transactionNames"></param>
  /// <param name="transactionGroups"></param>
  /// <param name="user"></param>
  /// <param name="correlationId"></param>
  /// <returns>Matched Dictionary</returns>
  Task<Result<bool>> MatchTransactionGroup(List<string> transactionNames, List<string> existingGroups, string userId, string correlationId);
}
