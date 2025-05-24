using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.Dtos.InvestmentDtos;

public class UpdateInvestmentDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public Money Value { get; set; } = new();
  public string? Description { get; set; }
}
