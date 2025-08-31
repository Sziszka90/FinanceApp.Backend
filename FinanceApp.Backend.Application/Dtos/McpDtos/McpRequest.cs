namespace FinanceApp.Backend.Application.Dtos.McpDtos;

public class McpRequest
{
  public required string ToolName { get; set; }
  public required Dictionary<string, object> Parameters { get; set; }
}
