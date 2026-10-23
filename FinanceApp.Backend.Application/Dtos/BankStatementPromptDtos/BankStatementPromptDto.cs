namespace FinanceApp.Backend.Application.Dtos.BankStatementPromptDtos;

public class BankStatementPromptDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Instructions { get; set; } = string.Empty;
  public bool IsSelected { get; set; }
}
