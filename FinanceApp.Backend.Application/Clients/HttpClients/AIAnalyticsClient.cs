using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Clients.HttpClients;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.Clients;

public class AIAnalyticsClient : HttpClientBase<IAIAnalyticsClient>, IAIAnalyticsClient
{
  public AIAnalyticsClient(
    ILogger<IAIAnalyticsClient> logger,
    HttpClient httpClient) : base(logger, httpClient)
  { }

  public async Task<Result<bool>> WakeupAsync()
  {
    await PostAsync("/wakeup");
    return Result.Success(true);
  }
}
