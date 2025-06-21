using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Dtos.UserDtos;

public class UpdateUserDto
{
  public Guid Id { get; set; }
  public string UserName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string? Password { get; set; }
  public CurrencyEnum BaseCurrency { get; set; }
}
