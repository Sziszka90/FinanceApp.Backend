using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos.ExpenseTransactionDtos;

public class GetExpenseTransactionDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Money Value { get; set; } = new();
  public DateTimeOffset? DueDate { get; set; }
  public int? Priority { get; set; }
  public GetExpenseTransactionGroupDto? TransactionGroup { get; set; }
}
