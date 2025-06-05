using FinanceApp.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace FinanceApp.Application.Dtos.TransactionGroupDtos;

public class UpdateTransactionGroupDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string? Description { get; set; }
  public string? GroupIcon { get; set; }
  public Money? Limit { get; set; }
}
