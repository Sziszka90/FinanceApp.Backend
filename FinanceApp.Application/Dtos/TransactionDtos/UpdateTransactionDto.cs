using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos.TransactionDtos;

public class UpdateTransactionDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Money Value { get; set; } = new();
  public TransactionTypeEnum TransactionType { get; set; }
  public DateTimeOffset TransactionDate { get; set; }
  public Guid? TransactionGroupId { get; set; } = null!;
}
