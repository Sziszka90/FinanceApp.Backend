
namespace FinanceApp.Backend.Application.Dtos.UserDtos;

public class UpdatePasswordRequest
{
  public string Password { get; set; } = string.Empty;
  public string Token { get; set; } = string.Empty;
}
