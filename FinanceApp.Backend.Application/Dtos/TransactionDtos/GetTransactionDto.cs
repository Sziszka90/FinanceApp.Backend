using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Dtos.TransactionDtos;

public class GetTransactionDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Money Value { get; set; } = new();
  public TransactionTypeEnum TransactionType { get; set; }
  public DateTimeOffset TransactionDate { get; set; }
  public GetTransactionGroupDto? TransactionGroup { get; set; }
}
