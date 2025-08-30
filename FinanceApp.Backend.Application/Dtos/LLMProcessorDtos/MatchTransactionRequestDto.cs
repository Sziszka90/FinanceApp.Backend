namespace FinanceApp.Backend.Application.Dtos.LLMProcessorDtos;

public class MatchTransactionRequestDto
{
  public required string UserId { get; set; }
  public required List<string> TransactionGroupNames { get; set; }
  public required List<string> TransactionNames { get; set; }
  public required string CorrelationId { get; set; }
}
