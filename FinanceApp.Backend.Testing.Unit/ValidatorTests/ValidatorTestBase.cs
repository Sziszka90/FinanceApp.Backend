namespace FinanceApp.Backend.Testing.Unit.ValidatorTests;

/// <summary>
/// Base class for validator tests providing common test utilities and helpers.
/// </summary>
public abstract class ValidatorTestBase
{
  /// <summary>
  /// Creates a string of the specified length for testing string length validations.
  /// </summary>
  /// <param name="length">The desired length of the string</param>
  /// <param name="character">The character to repeat (default: 'a')</param>
  /// <returns>A string of the specified length</returns>
  protected static string CreateStringOfLength(int length, char character = 'a')
  {
    return new string(character, length);
  }

  /// <summary>
  /// Creates a valid test email address.
  /// </summary>
  /// <param name="localPart">The local part of the email (before @)</param>
  /// <param name="domain">The domain part of the email (after @)</param>
  /// <returns>A valid email address</returns>
  protected static string CreateValidEmail(string localPart = "test", string domain = "example.com")
  {
    return $"{localPart}@{domain}";
  }

  /// <summary>
  /// Creates a password that meets common complexity requirements.
  /// </summary>
  /// <param name="length">The desired length (minimum 8)</param>
  /// <returns>A password with uppercase, lowercase, digit, and special character</returns>
  protected static string CreateValidPassword(int length = 12)
  {
    if (length < 8)
    {
      length = 8;
    }

    var basePassword = "Password123@";
    if (length <= basePassword.Length)
    {
      return basePassword.Substring(0, length);
    }

    return basePassword + new string('x', length - basePassword.Length);
  }
}
