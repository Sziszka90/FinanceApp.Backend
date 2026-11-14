using System.Text.Json.Serialization;

namespace FinanceApp.Backend.Application.Dtos.AIAnalytics;

public sealed class MonthlyInsightsRequestDto
{
  [JsonPropertyName("year")]
  public int Year { get; set; }

  [JsonPropertyName("month")]
  public int Month { get; set; }

  [JsonPropertyName("lookbackMonths")]
  public int LookbackMonths { get; set; } = 6;
}
