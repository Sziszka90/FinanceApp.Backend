using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos;

public class CreateExpenseTransactionDto
{
  #region Properties

  public string Name { get; set; } = string.Empty;

  public string? Description { get; set; }

  public Money Value { get; set; } = new();

  public DateTimeOffset? DueDate { get; set; }

  public int? Priority { get; set; }

  public Guid? TransactionGroupId { get; set; } = null!;

  #endregion
}