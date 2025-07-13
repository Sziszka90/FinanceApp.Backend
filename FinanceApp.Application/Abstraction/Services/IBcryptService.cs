
namespace FinanceApp.Application.Abstraction.Services;

public interface IBcryptService
{
  /// <summary>
  /// Verifies if the provided password matches the hashed password.
  /// </summary>
  /// <param name="password">The plain text password to verify.</param>
  /// <param name="hash">The hashed password to compare against.</param>
  /// <returns>True if the password matches the hash, otherwise false.</returns>
  bool Verify(string password, string hash);

  /// <summary>
  /// Hashes the provided password using BCrypt.
  /// </summary>
  /// <param name="password">The plain text password to hash.</param>
  /// <returns>The hashed password.</returns>
  string Hash(string password);
}
