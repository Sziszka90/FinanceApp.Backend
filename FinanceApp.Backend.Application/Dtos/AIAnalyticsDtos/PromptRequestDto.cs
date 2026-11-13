namespace FinanceApp.Backend.Application.Dtos.AIAnalyticsDtos;

public class PromptRequestDto
{
  public required string Prompt { get; set; }
  public required string CorrelationId { get; set; }
}
