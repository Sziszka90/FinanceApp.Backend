using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos;

public class GetSavingDto
{
  #region Properties

  public Guid Id { get; set; }
  public string Name { get; set; } = null!;

  public string? Description { get; set; }

  public Money Amount { get; set; } = new();

  public SavingTypeEnum Type { get; set; }

  public DateTimeOffset? DueDate { get; set; }

  #endregion
}