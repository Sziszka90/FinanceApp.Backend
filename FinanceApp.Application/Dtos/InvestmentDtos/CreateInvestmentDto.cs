using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos.InvestmentDtos;

public class CreateInvestmentDto
{
  public string Name { get; set; } = string.Empty;
  public Money Value { get; set; } = new();
  public string? Description { get; set; }
}
