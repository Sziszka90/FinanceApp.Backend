namespace FinanceApp.Domain.Options;

public class AuthenticationSettings
{
  /// <summary>
  /// Secret key used for signing JWT tokens.
  /// </summary>
  public string SecretKey { get; set; } = string.Empty;

  /// <summary>
  /// The issuer of the JWT token.
  /// </summary>
  public string Issuer { get; set; } = string.Empty;

  /// <summary>
  /// The audience for the JWT token.
  /// </summary>
  public string Audience { get; set; } = string.Empty;
}
