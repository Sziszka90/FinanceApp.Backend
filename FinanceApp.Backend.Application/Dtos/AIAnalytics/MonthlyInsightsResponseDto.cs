namespace FinanceApp.Backend.Application.Dtos.AIAnalytics;

public sealed class MonthlyInsightsResponseDto
{
  public int Year { get; set; }
  public int Month { get; set; }
  public string? Currency { get; set; }
  public List<MonthlyInsightCardDto> Insights { get; set; } = [];
}
