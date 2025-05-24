using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos.InvestmentDtos;

public class UpdateInvestmentDto
{
  #region Properties

  public Guid Id { get; set; }

  public string Name { get; set; } = string.Empty;

  public Money Amount { get; set; } = new();

  public string? Description { get; set; }

  #endregion
}
