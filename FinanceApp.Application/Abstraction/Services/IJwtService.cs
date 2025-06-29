namespace FinanceApp.Application.Abstraction.Services;

public interface IJwtService
{
  public string GenerateToken(string email);
  public bool ValidateToken(string token);
  public void InvalidateToken(string token);
  public bool IsTokenInvalidated(string token);
  public string? GetUserEmailFromToken(string token);
}
