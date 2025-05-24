using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos.SavingDtos;

public class CreateSavingDto
{
  #region Properties

  public string Name { get; set; } = null!;

  public string? Description { get; set; }

  public Money Amount { get; set; } = new();

  public SavingTypeEnum Type { get; set; }

  public DateTimeOffset? DueDate { get; set; }

  #endregion
}
