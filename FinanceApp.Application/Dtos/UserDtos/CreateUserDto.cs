using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Dtos.UserDtos;

public class CreateUserDto
{
  #region Properties

  public string UserName { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public CurrencyEnum BaseCurrency { get; set; }

  #endregion
}
