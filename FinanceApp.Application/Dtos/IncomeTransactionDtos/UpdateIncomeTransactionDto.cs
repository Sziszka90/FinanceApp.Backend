using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos;

public class UpdateIncomeTransactionDto
{
  #region Properties

  public Guid Id { get; set; }

  public string Name { get; set; } = string.Empty;

  public string? Description { get; set; }

  public Money Value { get; set; } = new();

  public DateTimeOffset? DueDate { get; set; }

  public Guid? TransactionGroupId { get; set; } = null!;

  #endregion
}
