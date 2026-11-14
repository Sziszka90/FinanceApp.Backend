using System.Net.Http.Json;
using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Clients.HttpClients;
using FinanceApp.Backend.Application.Dtos.AIAnalytics;
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

  public async Task<MonthlyInsightsResponseDto> GenerateMonthlyInsightsAsync(
    MonthlyInsightsRequestDto request,
    string userAccessToken,
    CancellationToken cancellationToken)
  {
    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/aianalytics/monthly-insights")
    {
      Content = JsonContent.Create(request)
    };
    httpRequest.Headers.Add("X-Backend-User-Token", userAccessToken);

    using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
      _logger.LogError("AI Analytics monthly insights request failed with status code {StatusCode}", response.StatusCode);
      throw new HttpRequestException("AI Analytics monthly insights request failed.", null, response.StatusCode);
    }

    var result = await response.Content.ReadFromJsonAsync<MonthlyInsightsResponseDto>(
      new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
      cancellationToken);
    return result ?? throw new JsonException("AI Analytics returned an empty monthly insights response.");
  }
}
