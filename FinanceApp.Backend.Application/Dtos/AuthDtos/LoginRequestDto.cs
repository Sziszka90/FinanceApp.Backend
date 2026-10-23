namespace FinanceApp.Backend.Application.Dtos.AuthDtos;

public class LoginRequestDto
{
  public string Email { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
}
