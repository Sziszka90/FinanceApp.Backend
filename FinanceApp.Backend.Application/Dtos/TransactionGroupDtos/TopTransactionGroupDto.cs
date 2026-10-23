using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;

public class TopTransactionGroupDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public required Money TotalAmount { get; set; }
  public int TransactionCount { get; set; }
}
