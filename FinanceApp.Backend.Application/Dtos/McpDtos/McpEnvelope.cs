namespace FinanceApp.Backend.Application.Dtos.McpDtos;

public class McpEnvelope
{
  public required string ToolName { get; set; }
  public required object Payload { get; set; }
}
