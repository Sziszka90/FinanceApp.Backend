using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Enums;

namespace FinanceApp.Backend.Application.Services;

public interface ITokenService
{
  /// <summary>
  /// Validates the provided token asynchronously.
  /// </summary>
  /// <param name="token">The token to validate.</param>
  /// <param name="tokenType">The type of the token.</param>
  /// <returns>True if the token is valid, otherwise false.</returns>
  Task<Result<bool>> ValidateTokenAsync(string token, TokenType tokenType);

  /// <summary>
  /// Checks if a token is valid for the specified user email and token type.
  /// </summary>
  /// <param name="token">The token to check.</param>
  /// <param name="tokenType">The type of the token.</param>
  /// <returns>True if the token is valid, otherwise false.</returns>
  Task<bool> IsTokenValidAsync(string token, TokenType tokenType);

  /// <summary>
  /// Checks if a refresh token is valid for the specified user email and token type.
  /// </summary>
  /// <param name="token">The token to check.</param>
  /// <returns>True if the token is valid, otherwise false.</returns>
  Task<Result<bool>> IsRefreshTokenValidAsync(string token);

  /// <summary>
  /// Generates a token of the specified type asynchronously.
  /// </summary>
  /// <param name="userEmail">The email of the user for whom the token is generated.</param>
  /// <param name="tokenType">The type of the token to generate.</param>
  /// <returns>Result containing the generated token.</returns>
  Task<Result<string>> GenerateTokenAsync(string userEmail, TokenType tokenType);

  /// <summary>
  /// Generates a refresh token of the specified type asynchronously.
  /// </summary>
  /// <param name="userEmail">The email of the user for whom the refresh token is generated.</param>
  /// <returns>Result containing the generated token.</returns>
  Task<Result<string>> GenerateRefreshTokenAsync(string userEmail);

  /// <summary>
  /// Gets the user email from the provided token.
  /// </summary>
  /// <param name="token">The token to extract the email from.</param>
  /// <returns>The user email associated with the token.</returns>
  string GetEmailFromTokenAsync(string token);

  /// <summary>
  /// Invalidates a token by removing it from the cache.
  /// </summary>
  /// <param name="token">The token to invalidate.</param>
  /// <param name="tokenType">The type of the token to invalidate.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task InvalidateTokenAsync(string token, TokenType tokenType);

  /// <summary>
  /// Invalidates a refresh token by removing it from the cache.
  /// </summary>
  /// <param name="token">The token to invalidate.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task InvalidateRefreshTokenAsync(string token);
}
