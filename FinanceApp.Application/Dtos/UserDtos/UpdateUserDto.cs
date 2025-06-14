using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Dtos.UserDtos;

public class UpdateUserDto
{
  public Guid Id { get; set; }
  public CurrencyEnum BaseCurrency { get; set; }
}
