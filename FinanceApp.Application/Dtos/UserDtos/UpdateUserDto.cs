using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Dtos.UserDtos;

public class UpdateUserDto
{
  public Guid Id { get; set; }
  public string? UserName { get; set; }
  public string? Password { get; set; }
  public CurrencyEnum? BaseCurrency { get; set; }
}
