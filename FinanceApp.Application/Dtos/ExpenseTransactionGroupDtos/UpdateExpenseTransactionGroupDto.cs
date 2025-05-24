using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;

public class UpdateExpenseTransactionGroupDto
{
  #region Properties

  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? Icon { get; set; }
  public Money? Limit { get; set; }

  #endregion
}
