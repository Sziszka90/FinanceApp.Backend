using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos.IncomeTransactionDtos;

public class GetIncomeTransactionDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public Money Value { get; set; } = new();
  public DateTimeOffset? DueDate { get; set; }
  public GetIncomeTransactionGroupDto? TransactionGroup { get; set; }
}
