using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.Dtos.TransactionDtos;

public class TransactionFilter
{
  public string? TransactionGroupName { get; set; }
  public string? TransactionName { get; set; }
  public DateTimeOffset? TransactionDate { get; set; }
  public TransactionTypeEnum? TransactionType { get; set; }
  public string? OrderBy { get; set; }
  public bool? Ascending { get; set; }
}
