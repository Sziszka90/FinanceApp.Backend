using FinanceApp.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace FinanceApp.Application.Dtos.TransactionDtos;

public class CreateTransactionDto
{
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Money Value { get; set; } = new();
  public DateTimeOffset TransactionDate { get; set; }
  public TransactionTypeEnum TransactionType { get; set; }
  public string? TransactionGroupId { get; set; } = null!;
}
