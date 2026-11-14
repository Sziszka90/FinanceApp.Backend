namespace FinanceApp.Backend.Application.Dtos.AIAnalytics;

public sealed class MonthlyInsightCardDto
{
  public string Category { get; set; } = string.Empty;
  public string Severity { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public List<Guid> TransactionIds { get; set; } = [];
  public decimal? Amount { get; set; }
  public string? Currency { get; set; }
}
