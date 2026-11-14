using FinanceApp.Backend.Application.Dtos.AIAnalytics;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.Abstraction.Clients;

public interface IAIAnalyticsClient
{
  /// <summary>
  /// Wakeup AI Analytics service.
  /// </summary>
  /// <returns>True if the service is awake, false otherwise.</returns>
  Task<Result<bool>> WakeupAsync();

  Task<MonthlyInsightsResponseDto> GenerateMonthlyInsightsAsync(
    MonthlyInsightsRequestDto request,
    string userAccessToken,
    CancellationToken cancellationToken);

}
