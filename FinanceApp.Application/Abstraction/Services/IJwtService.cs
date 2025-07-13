namespace FinanceApp.Application.Abstraction.Services;

public interface IJwtService
{
  /// <summary>
  /// Generates a JWT token for the given email address.
  /// </summary>
  /// <param name="email"></param>
  /// <returns>Generated Token</returns>
  string GenerateToken(string email);

  /// <summary>
  /// Validates the given JWT token.
  /// </summary>
  /// <param name="token"></param>
  /// <returns>Boolean - success or failure</returns>
  bool ValidateToken(string token);

  /// <summary>
  /// Invalidates the given JWT token, preventing it from being used for authentication.
  /// </summary>
  /// <param name="token"></param>
  /// <returns></returns>
  void InvalidateToken(string token);

  /// <summary>
  /// Checks if the given JWT token has been invalidated.
  /// </summary>
  /// <param name="token"></param>
  /// <returns>Boolean - success or failure</returns>
  bool IsTokenInvalidated(string token);

  /// <summary>
  /// Extracts the email address from the given JWT token.
  /// </summary>
  /// <param name="token"></param>
  /// <returns>User Email</returns>
  string? GetUserEmailFromToken(string token);
}
