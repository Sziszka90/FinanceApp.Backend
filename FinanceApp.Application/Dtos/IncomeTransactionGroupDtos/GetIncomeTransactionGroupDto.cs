namespace FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;

public class GetIncomeTransactionGroupDto
{
  #region Properties

  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? Icon { get; set; }

  #endregion
}