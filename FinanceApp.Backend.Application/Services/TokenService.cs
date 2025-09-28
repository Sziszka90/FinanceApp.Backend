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
  private readonly ITokenCacheManager _cacheManager;
  private readonly IAsyncPolicy _tokenGenerationRetryPolicy;

  public TokenService(
    ILogger<TokenService> logger,
    IJwtService jwtService,
    ITokenCacheManager cacheManager)
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

  public async Task<Result<bool>> IsTokenValidAsync(string token, TokenType tokenType, CancellationToken cancellationToken = default)
  {
    if (!_jwtService.ValidateToken(token))
    {
      _logger.LogWarning("Invalid token provided: {Token}", token);
      var result = await InvalidateTokenAsync(token, tokenType, cancellationToken);

      if (!result.IsSuccess)
      {
        _logger.LogError("Error while invalidating token: {Token}. Error: {Error}", token, result.ApplicationError?.Message);
        return Result.Failure<bool>(result.ApplicationError!);
      }
      return Result.Success(false);
    }

    switch (tokenType)
    {
      case TokenType.Login:
        try
        {
          var isLoginTokenValid = await _cacheManager.IsLoginTokenValidAsync(token, cancellationToken);
          if (!isLoginTokenValid)
          {
            _logger.LogWarning("Invalid login token in cache: {Token}", token);
            return Result.Success(false);
          }
          _logger.LogInformation("Login token validated successfully: {Token}", token);
          return Result.Success(true);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error while validating login token: {Token}", token);
          return Result.Failure<bool>(ApplicationError.CacheConnectionError());
        }

      case TokenType.PasswordReset:
        try
        {
          var isPasswordResetTokenValid = await _cacheManager.IsPasswordResetTokenValidAsync(token, cancellationToken);
          if (!isPasswordResetTokenValid)
          {
            _logger.LogWarning("Invalid password reset token in cache: {Token}", token);
            return Result.Success(false);
          }
          _logger.LogInformation("Password reset token validated successfully: {Token}", token);
          return Result.Success(true);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error while validating password reset token: {Token}", token);
          return Result.Failure<bool>(ApplicationError.CacheConnectionError());
        }

      case TokenType.EmailConfirmation:
        try
        {
          var isEmailConfirmationTokenValid = await _cacheManager.IsEmailConfirmationTokenValidAsync(token, cancellationToken);
          if (!isEmailConfirmationTokenValid)
          {
            _logger.LogWarning("Invalid email confirmation token in cache: {Token}", token);
            return Result.Success(false);
          }
          _logger.LogInformation("Email confirmation token validated successfully: {Token}", token);
          return Result.Success(true);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error while validating email confirmation token: {Token}", token);
          return Result.Failure<bool>(ApplicationError.CacheConnectionError());
        }
      default:
        _logger.LogError("Unknown token type: {TokenType}", tokenType);
        return Result.Failure<bool>(ApplicationError.UnknownTokenTypeError(tokenType.ToString()));
    }
  }

  public async Task<Result<string>> GenerateTokenAsync(string userEmail, TokenType tokenType, CancellationToken cancellationToken = default)
  {
    try
    {
      var token = await _tokenGenerationRetryPolicy.ExecuteAsync(async () =>
      {
        switch (tokenType)
        {
          case TokenType.Login:
            var generatedToken = _jwtService.GenerateToken(userEmail);

            if (await _cacheManager.LoginTokenExistsAsync(generatedToken, cancellationToken))
            {
              throw new InvalidOperationException("Token collision detected");
            }
            await _cacheManager.SaveLoginTokenAsync(generatedToken, cancellationToken);
            return generatedToken;

          case TokenType.PasswordReset:
            var generatedPasswordResetToken = _jwtService.GeneratePasswordResetToken(userEmail);

            if (await _cacheManager.PasswordResetTokenExistsAsync(generatedPasswordResetToken, cancellationToken))
            {
              throw new InvalidOperationException("Token collision detected");
            }
            await _cacheManager.SavePasswordResetTokenAsync(generatedPasswordResetToken, cancellationToken);
            return generatedPasswordResetToken;

          case TokenType.EmailConfirmation:
            var generatedEmailConfirmationToken = _jwtService.GenerateEmailConfirmationToken(userEmail);

            if (await _cacheManager.EmailConfirmationTokenExistsAsync(generatedEmailConfirmationToken, cancellationToken))
            {
              throw new InvalidOperationException("Token collision detected");
            }
            await _cacheManager.SaveEmailConfirmationTokenAsync(generatedEmailConfirmationToken, cancellationToken);
            return generatedEmailConfirmationToken;

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

  public async Task<Result<bool>> ValidateTokenAsync(string token, TokenType tokenType, CancellationToken cancellationToken = default)
  {
    if (!_jwtService.ValidateToken(token))
    {
      _logger.LogWarning("Invalid token provided: {Token}", token);
      await InvalidateTokenAsync(token, tokenType, cancellationToken);
      return Result.Failure<bool>(ApplicationError.InvalidTokenError());
    }

    switch (tokenType)
    {
      case TokenType.Login:
        if (!await _cacheManager.IsLoginTokenValidAsync(token, cancellationToken))
        {
          _logger.LogWarning("Invalid login token in cache: {Token}", token);
          return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
        }
        await InvalidateTokenAsync(token, tokenType, cancellationToken);
        _logger.LogInformation("Login token validated and invalidated: {Token}", token);
        return Result.Success(true);

      case TokenType.PasswordReset:
        if (!await _cacheManager.IsPasswordResetTokenValidAsync(token, cancellationToken))
        {
          _logger.LogWarning("Invalid password reset token in cache: {Token}", token);
          return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
        }
        await InvalidateTokenAsync(token, tokenType, cancellationToken);
        _logger.LogInformation("Password reset token validated and invalidated: {Token}", token);
        return Result.Success(true);

      case TokenType.EmailConfirmation:
        if (!await _cacheManager.IsEmailConfirmationTokenValidAsync(token, cancellationToken))
        {
          _logger.LogWarning("Invalid email confirmation token in cache: {Token}", token);
          return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
        }
        await InvalidateTokenAsync(token, tokenType, cancellationToken);
        _logger.LogInformation("Email confirmation token validated and invalidated: {Token}", token);
        return Result.Success(true);

      default:
        _logger.LogError("Unknown token type: {TokenType}", tokenType);
        return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
    }
  }

  public string GetEmailFromToken(string token)
  {
    return _jwtService.GetUserEmailFromToken(token) ?? String.Empty;
  }

  public async Task<Result> InvalidateTokenAsync(string token, TokenType tokenType, CancellationToken cancellationToken = default)
  {
    switch (tokenType)
    {
      case TokenType.Login:
        try
        {
          await _cacheManager.InvalidateLoginTokenAsync(token, cancellationToken);
          _logger.LogInformation("Login token invalidated: {Token}", token);
          return Result.Success();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error while invalidating login token: {Token}", token);
          return Result.Failure(ApplicationError.CacheConnectionError());
        }

      case TokenType.PasswordReset:
        try
        {
          await _cacheManager.InvalidatePasswordResetTokenAsync(token, cancellationToken);
          _logger.LogInformation("Password reset token invalidated: {Token}", token);
          return Result.Success();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error while invalidating password reset token: {Token}", token);
          return Result.Failure(ApplicationError.CacheConnectionError());
        }

      case TokenType.EmailConfirmation:
        try
        {
          await _cacheManager.InvalidateEmailConfirmationTokenAsync(token, cancellationToken);
          _logger.LogInformation("Email confirmation token invalidated: {Token}", token);
          return Result.Success();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error while invalidating email confirmation token: {Token}", token);
          return Result.Failure(ApplicationError.CacheConnectionError());
        }

      default:
        _logger.LogError("Unknown token type: {TokenType}", tokenType);
        return Result.Failure(ApplicationError.UnknownTokenTypeError(tokenType.ToString()));
    }
  }

  public async Task<Result<string>> GenerateRefreshTokenAsync(string userEmail, CancellationToken cancellationToken = default)
  {
    try
    {
      var token = await _tokenGenerationRetryPolicy.ExecuteAsync(async () =>
      {
        var generatedToken = _jwtService.GenerateRefreshToken(userEmail);

        if (await _cacheManager.RefreshTokenExistsAsync(generatedToken, cancellationToken))
        {
          throw new InvalidOperationException("RefreshToken collision detected");
        }
        await _cacheManager.SaveRefreshTokenAsync(generatedToken, cancellationToken);
        return generatedToken;

      });

      _logger.LogInformation("Refresh token generated and saved: {Token}", token);
      return Result.Success(token);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to generate unique refresh token after all retry attempts");
      return Result.Failure<string>(ApplicationError.TokenGenerationError());
    }
  }

  public async Task<Result<bool>> IsRefreshTokenValidAsync(string token, CancellationToken cancellationToken = default)
  {
    if (!_jwtService.ValidateToken(token))
    {
      _logger.LogWarning("Invalid token provided: {Token}", token);
      try
      {
        await _cacheManager.InvalidateLoginTokenAsync(token, cancellationToken);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error while invalidating login token: {Token}", token);
        return Result.Failure<bool>(ApplicationError.CacheConnectionError());
      }
      return Result.Failure<bool>(ApplicationError.InvalidTokenError("RefreshToken"));
    }

    try
    {
      var isRefreshTokenValid = await _cacheManager.IsRefreshTokenValidAsync(token, cancellationToken);

      if (!isRefreshTokenValid)
      {
        _logger.LogWarning("Invalid refresh token in cache: {Token}", token);
        return Result.Failure<bool>(ApplicationError.InvalidTokenError("RefreshToken"));
      }
      _logger.LogInformation("Refresh token validated: {Token}", token);
      return Result.Success(true);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error while validating refresh token: {Token}", token);
      return Result.Failure<bool>(ApplicationError.CacheConnectionError());
    }

  }

  public async Task<Result> InvalidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
  {
    try
    {
      await _cacheManager.InvalidateRefreshTokenAsync(token, cancellationToken);
      _logger.LogInformation("Refresh token invalidated: {Token}", token);
      return Result.Success();
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error while invalidating refresh token: {Token}", token);
      return Result.Failure(ApplicationError.CacheConnectionError());
    }
  }
}
