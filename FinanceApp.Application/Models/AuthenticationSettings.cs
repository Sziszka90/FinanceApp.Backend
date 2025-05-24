namespace FinanceApp.Application.Models;

public class AuthenticationSettings
{
  public string SecretKey { get; set; } = string.Empty;
  public string Issuer { get; set; } = string.Empty;
  public string Audience { get; set; } = string.Empty;
}
