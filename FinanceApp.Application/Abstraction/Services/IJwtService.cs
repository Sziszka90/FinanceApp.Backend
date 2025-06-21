namespace FinanceApp.Application.Abstraction.Services;

public interface IJwtService
{
  public string GenerateToken(string username);
  public bool ValidateToken(string token);
}
