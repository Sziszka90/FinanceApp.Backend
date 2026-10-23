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
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>True if the token is valid, otherwise false.</returns>
  Task<Result<bool>> ValidateTokenAsync(string token, TokenType tokenType, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if a token is valid for the specified user email and token type.
  /// </summary>
  /// <param name="token">The token to check.</param>
  /// <param name="tokenType">The type of the token.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>True if the token is valid, otherwise false.</returns>
  Task<Result<bool>> IsTokenValidAsync(string token, TokenType tokenType, CancellationToken cancellationToken = default);

  /// <summary>
  /// Checks if a refresh token is valid for the specified user email and token type.
  /// </summary>
  /// <param name="token">The token to check.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>True if the token is valid, otherwise false.</returns>
  Task<Result<bool>> IsRefreshTokenValidAsync(string token, CancellationToken cancellationToken = default);

  /// <summary>
  /// Generates a token of the specified type asynchronously.
  /// </summary>
  /// <param name="userEmail">The email of the user for whom the token is generated.</param>
  /// <param name="tokenType">The type of the token to generate.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>Result containing the generated token.</returns>
  Task<Result<string>> GenerateTokenAsync(string userEmail, TokenType tokenType, CancellationToken cancellationToken = default);

  /// <summary>
  /// Generates a refresh token of the specified type asynchronously.
  /// </summary>
  /// <param name="userEmail">The email of the user for whom the refresh token is generated.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>Result containing the generated token.</returns>
  Task<Result<string>> GenerateRefreshTokenAsync(string userEmail, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the user email from the provided token.
  /// </summary>
  /// <param name="token">The token to extract the email from.</param>
  /// <returns>The user email associated with the token.</returns>
  string GetEmailFromToken(string token);

  /// <summary>
  /// Invalidates a token by removing it from the cache.
  /// </summary>
  /// <param name="token">The token to invalidate.</param>
  /// <param name="tokenType">The type of the token to invalidate.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task<Result> InvalidateTokenAsync(string token, TokenType tokenType, CancellationToken cancellationToken = default);

  /// <summary>
  /// Invalidates a refresh token by removing it from the cache.
  /// </summary>
  /// <param name="token">The token to invalidate.</param>
  /// <param name="cancellationToken">A cancellation token.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task<Result> InvalidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}
