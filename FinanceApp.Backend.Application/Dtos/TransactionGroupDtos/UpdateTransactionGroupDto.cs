namespace FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;

public class UpdateTransactionGroupDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? GroupIcon { get; set; }
}
