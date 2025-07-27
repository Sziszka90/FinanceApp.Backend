using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Polly;

namespace FinanceApp.Backend.Application.Services;

public class TokenService : ITokenService
{
  private readonly ILogger<TokenService> _logger;
  private readonly IJwtService _jwtService;
  private readonly ICacheManager _cacheManager;
  private readonly IAsyncPolicy _tokenGenerationRetryPolicy;

  public TokenService(
    ILogger<TokenService> logger,
    IJwtService jwtService,
    ICacheManager cacheManager)
  {
    _logger = logger;
    _jwtService = jwtService;
    _cacheManager = cacheManager;

    _tokenGenerationRetryPolicy = Policy
      .Handle<InvalidOperationException>()
      .WaitAndRetryAsync(
        retryCount: 4,
        sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(Math.Min(Math.Pow(2, retryAttempt) * 10, 100)),
        onRetry: (exception, timespan, retryCount, context) =>
        {
          _logger.LogInformation("Token collision detected on attempt {Attempt}/5. Generating new token in {Delay}ms...",
          retryCount, timespan.TotalMilliseconds);
        });
  }

  public async Task<bool> IsTokenValidAsync(string token, TokenType tokenType)
  {
    if (!_jwtService.ValidateToken(token))
    {
      _logger.LogWarning("Invalid token provided: {Token}", token);
      await InvalidateTokenAsync(token, tokenType);
      return false;
    }

    switch (tokenType)
    {
      case TokenType.Login:
        if (!await _cacheManager.IsLoginTokenValidAsync(token))
        {
          _logger.LogWarning("Invalid login token in cache: {Token}", token);
          return false;
        }
        _logger.LogInformation("Login token validated successfully: {Token}", token);
        return true;

      case TokenType.PasswordReset:
        if (!await _cacheManager.IsPasswordResetTokenValidAsync(token))
        {
          _logger.LogWarning("Invalid password reset token in cache: {Token}", token);
          return false;
        }
        _logger.LogInformation("Password reset token validated successfully: {Token}", token);
        return true;

      case TokenType.EmailConfirmation:
        if (!await _cacheManager.IsEmailConfirmationTokenValidAsync(token))
        {
          _logger.LogWarning("Invalid email confirmation token in cache: {Token}", token);
          return false;
        }
        _logger.LogInformation("Email confirmation token validated successfully: {Token}", token);
        return true;

      default:
        _logger.LogError("Unknown token type: {TokenType}", tokenType);
        return false;
    }
  }

  public async Task<Result<string>> GenerateTokenAsync(string userEmail, TokenType tokenType)
  {
    try
    {
      var token = await _tokenGenerationRetryPolicy.ExecuteAsync(async () =>
      {
        var generatedToken = _jwtService.GenerateToken(userEmail);

        switch (tokenType)
        {
          case TokenType.Login:
            if (await _cacheManager.LoginTokenExistsAsync(generatedToken))
            {
              throw new InvalidOperationException("Token collision detected");
            }
            await _cacheManager.SaveLoginTokenAsync(generatedToken);
            return generatedToken;

          case TokenType.PasswordReset:
            if (await _cacheManager.PasswordResetTokenExistsAsync(generatedToken))
            {
              throw new InvalidOperationException("Token collision detected");
            }
            await _cacheManager.SavePasswordResetTokenAsync(generatedToken);
            return generatedToken;

          case TokenType.EmailConfirmation:
            if (await _cacheManager.EmailConfirmationTokenExistsAsync(generatedToken))
            {
              throw new InvalidOperationException("Token collision detected");
            }
            await _cacheManager.SaveEmailConfirmationTokenAsync(generatedToken);
            return generatedToken;

          default:
            throw new ArgumentException($"Unknown token type: {tokenType}");
        }
      });

      _logger.LogInformation("{TokenType} token generated and saved: {Token}", tokenType, token);
      return Result.Success(token);
    }
    catch (ArgumentException ex)
    {
      _logger.LogError(ex, "Unknown token type: {TokenType}", tokenType);
      return Result.Failure<string>(ApplicationError.UnknownTokenTypeError(tokenType.ToString()));
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to generate unique token after all retry attempts for token type: {TokenType}", tokenType);
      return Result.Failure<string>(ApplicationError.TokenGenerationError());
    }
  }

  public async Task<Result<bool>> ValidateTokenAsync(string token, TokenType tokenType)
  {
    if (!_jwtService.ValidateToken(token))
    {
      _logger.LogWarning("Invalid token provided: {Token}", token);
      await InvalidateTokenAsync(token, tokenType);
      return Result.Failure<bool>(ApplicationError.InvalidTokenError());
    }

    switch (tokenType)
    {
      case TokenType.Login:
        if (!await _cacheManager.IsLoginTokenValidAsync(token))
        {
          _logger.LogWarning("Invalid login token in cache: {Token}", token);
          return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
        }
        await InvalidateTokenAsync(token, tokenType);
        _logger.LogInformation("Login token validated and invalidated: {Token}", token);
        return Result.Success(true);

      case TokenType.PasswordReset:
        if (!await _cacheManager.IsPasswordResetTokenValidAsync(token))
        {
          _logger.LogWarning("Invalid password reset token in cache: {Token}", token);
          return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
        }
        await InvalidateTokenAsync(token, tokenType);
        _logger.LogInformation("Password reset token validated and invalidated: {Token}", token);
        return Result.Success(true);

      case TokenType.EmailConfirmation:
        if (!await _cacheManager.IsEmailConfirmationTokenValidAsync(token))
        {
          _logger.LogWarning("Invalid email confirmation token in cache: {Token}", token);
          return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
        }
        await InvalidateTokenAsync(token, tokenType);
        _logger.LogInformation("Email confirmation token validated and invalidated: {Token}", token);
        return Result.Success(true);

      default:
        _logger.LogError("Unknown token type: {TokenType}", tokenType);
        return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
    }
  }

  public string GetEmailFromTokenAsync(string token)
  {
    return _jwtService.GetUserEmailFromToken(token) ?? String.Empty;
  }

  public async Task InvalidateTokenAsync(string token, TokenType tokenType)
  {
    switch (tokenType)
    {
      case TokenType.Login:
        await _cacheManager.InvalidateLoginTokenAsync(token);
        break;

      case TokenType.PasswordReset:
        await _cacheManager.InvalidatePasswordResetTokenAsync(token);
        break;

      case TokenType.EmailConfirmation:
        await _cacheManager.InvalidateEmailConfirmationTokenAsync(token);
        break;

      default:
        _logger.LogError("Unknown token type: {TokenType}", tokenType);
        break;
    }
    _logger.LogInformation("Token invalidated: {Token}, Type: {TokenType}", token, tokenType);
  }
}
