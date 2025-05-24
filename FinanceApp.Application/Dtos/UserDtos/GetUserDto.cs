using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Dtos.UserDtos;

public class GetUserDto
{
  #region Properties

  public Guid Id { get; set; }
  public string UserName { get; set; } = string.Empty;
  public CurrencyEnum BaseCurrency { get; set; }

  #endregion
}
