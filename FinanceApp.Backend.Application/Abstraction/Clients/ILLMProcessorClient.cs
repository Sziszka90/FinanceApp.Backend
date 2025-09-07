using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface ILLMProcessorClient
{
  /// <summary>
  /// Matches transaction names with existing groups.
  /// </summary>
  /// <param name="userId"></param>
  /// <param name="transactionNames"></param>
  /// <param name="existingGroups"></param>
  /// <param name="correlationId"></param>
  /// <returns>Matched Dictionary</returns>
  Task<Result<bool>> MatchTransactionGroup(string userId, List<string> transactionNames, List<string> existingGroups, string correlationId);

  /// <summary>
  /// Wakeup LLM Processor service.
  /// </summary>
  /// <returns>True if the service is awake, false otherwise.</returns>
  Task<Result<bool>> WakeupAsync();

}
