namespace FinanceApp.Application.Dtos.TransactionGroupDtos;

public class CreateTransactionGroupDto
{
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? GroupIcon { get; set; }
}
