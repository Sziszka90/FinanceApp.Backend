using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos.SavingDtos;

public class CreateSavingDto
{
  public string Name { get; set; } = null!;
  public string? Description { get; set; }
  public Money Value { get; set; } = new();
  public SavingTypeEnum Type { get; set; }
  public DateTimeOffset? DueDate { get; set; }
}
