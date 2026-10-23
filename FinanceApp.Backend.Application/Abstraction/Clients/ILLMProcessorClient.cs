using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface ILLMProcessorClient
{
  /// <summary>
  /// Wakeup LLM Processor service.
  /// </summary>
  /// <returns>True if the service is awake, false otherwise.</returns>
  Task<Result<bool>> WakeupAsync();

}
