namespace FinanceApp.Application.LLMProcessorDtos;

public class LLMProcessorResponseDto
{
  public required string Status { get; set; }
  public required string CorrelationId { get; set; }
  public required string Message { get; set; }
}
