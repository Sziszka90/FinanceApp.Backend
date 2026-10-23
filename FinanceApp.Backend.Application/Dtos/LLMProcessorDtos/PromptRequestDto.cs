namespace FinanceApp.Backend.Application.Dtos.LLMProcessorDtos;

public class PromptRequestDto
{
  public required string Prompt { get; set; }
  public required string CorrelationId { get; set; }
}
