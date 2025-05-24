namespace FinanceApp.Application.Dtos.AuthDtos;

public class LoginRequestDto
{
  #region Properties

  public string UserName { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;

  #endregion
}
