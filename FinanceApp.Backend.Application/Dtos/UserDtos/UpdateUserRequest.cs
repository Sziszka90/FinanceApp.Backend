using FinanceApp.Backend.Domain.Enums;

namespace FinanceApp.Backend.Application.Dtos.UserDtos;

public class UpdateUserRequest
{
  public Guid Id { get; set; }
  public string? UserName { get; set; }
  public string? Password { get; set; }
  public CurrencyEnum? BaseCurrency { get; set; }
}
