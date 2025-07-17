namespace FinanceApp.Application.Application.Dtos.LLMProcessorDtos;

public class LLMProcessorRequestDto
{
  public required string UserId { get; set; }
  public required List<string> TransactionNames { get; set; }
  public required List<string> TransactionGroupNames { get; set; }
}
