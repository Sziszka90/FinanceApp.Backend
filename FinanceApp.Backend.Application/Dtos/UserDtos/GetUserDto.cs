using FinanceApp.Backend.Domain.Enums;

namespace FinanceApp.Backend.Application.Dtos.UserDtos;

public class GetUserDto
{
  public Guid Id { get; set; }
  public string UserName { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public CurrencyEnum BaseCurrency { get; set; }
}
