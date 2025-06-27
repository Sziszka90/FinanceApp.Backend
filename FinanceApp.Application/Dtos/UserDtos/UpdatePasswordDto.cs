
namespace FinanceApp.Application.Dtos.UserDtos;

public class UpdatePasswordDto
{
  public string Password { get; set; } = string.Empty;
  public string Token { get; set; } = string.Empty;
}
