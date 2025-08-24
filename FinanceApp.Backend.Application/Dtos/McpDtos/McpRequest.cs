namespace FinanceApp.Backend.Application.Dtos.McpDtos;

public class McpRequest
{
  public required string Name { get; set; }
  public required Dictionary<string, object> Arguments { get; set; }
}
