using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.Services;

public class TokenService : ITokenService
{
  private readonly ILogger<TokenService> _logger;
  private readonly IJwtService _jwtService;
  private readonly ICacheManager _cacheManager;

  public TokenService(
    ILogger<TokenService> logger,
    IJwtService jwtService,
    ICacheManager cacheManager)
  {
    _logger = logger;
    _jwtService = jwtService;
    _cacheManager = cacheManager;
  }

  public async Task<bool> IsTokenValidAsync(string token, TokenType tokenType)
  {
    if (!_jwtService.ValidateToken(token))
    {
      _logger.LogWarning("Invalid token provided: {Token}", token);
      return false;
    }

    switch (tokenType)
    {
      case TokenType.Login:
        if (await _cacheManager.IsLoginTokenValidAsync(token))
        {
          break;
        }
        _logger.LogWarning("Invalid login token in cache: {Token}", token);
        return false;

      case TokenType.PasswordReset:
        if (await _cacheManager.IsPasswordResetTokenValidAsync(token))
        {
          break;
        }
        _logger.LogWarning("Invalid password reset token in cache: {Token}", token);
        return false;

      case TokenType.EmailConfirmation:
        if (await _cacheManager.IsEmailConfirmationTokenValidAsync(token))
        {
          break;
        }
        _logger.LogWarning("Invalid email confirmation token in cache: {Token}", token);
        return false;

      default:
        _logger.LogError("Unknown token type: {TokenType}", tokenType);
        return false;
    }
    _logger.LogInformation("Token validated successfully: {Token}", token);
    return true;
  }

  public async Task<Result<string>> GenerateTokenAsync(string userEmail, TokenType tokenType)
  {
    var token = _jwtService.GenerateToken(userEmail);

    switch (tokenType)
    {
      case TokenType.Login:
        if (await _cacheManager.LoginTokenExistsAsync(token))
        {
          _logger.LogWarning("Token already exists in cache: {Token}", token);
          return Result.Failure<string>(ApplicationError.TokenAlreadyExistsError());
        }
        await _cacheManager.SaveLoginTokenAsync(token);
        _logger.LogInformation("Login token generated and saved: {Token}", token);
        return Result.Success(token);

      case TokenType.PasswordReset:
        if (await _cacheManager.PasswordResetTokenExistsAsync(token))
        {
          _logger.LogWarning("Password reset token already exists in cache: {Token}", token);
          return Result.Failure<string>(ApplicationError.TokenAlreadyExistsError());
        }
        await _cacheManager.SavePasswordResetTokenAsync(token);
        _logger.LogInformation("Password reset token generated and saved: {Token}", token);
        return Result.Success(token);

      case TokenType.EmailConfirmation:
        if (await _cacheManager.EmailConfirmationTokenExistsAsync(token))
        {
          _logger.LogWarning("Email confirmation token already exists in cache: {Token}", token);
          return Result.Failure<string>(ApplicationError.TokenAlreadyExistsError());
        }
        await _cacheManager.SaveEmailConfirmationTokenAsync(token);
        _logger.LogInformation("Email confirmation token generated and saved: {Token}", token);
        return Result.Success(token);

      default:
        _logger.LogError("Unknown token type: {TokenType}", tokenType);
        return Result.Failure<string>(ApplicationError.UnknownTokenTypeError(tokenType.ToString()));
    }
  }

  public async Task<Result<bool>> ValidateTokenAsync(string token, TokenType tokenType)
  {
    if (!_jwtService.ValidateToken(token))
    {
      _logger.LogWarning("Invalid token provided: {Token}", token);
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
        await _cacheManager.InvalidateLoginTokenAsync(token);
        _logger.LogInformation("Login token validated and invalidated: {Token}", token);
        return Result.Success(true);

      case TokenType.PasswordReset:
        if (!await _cacheManager.IsPasswordResetTokenValidAsync(token))
        {
          _logger.LogWarning("Invalid password reset token in cache: {Token}", token);
          return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
        }
        await _cacheManager.InvalidatePasswordResetTokenAsync(token);
        _logger.LogInformation("Password reset token validated and invalidated: {Token}", token);
        return Result.Success(true);

      case TokenType.EmailConfirmation:
        if (!await _cacheManager.IsEmailConfirmationTokenValidAsync(token))
        {
          _logger.LogWarning("Invalid email confirmation token in cache: {Token}", token);
          return Result.Failure<bool>(ApplicationError.InvalidTokenError(tokenType.ToString()));
        }
        await _cacheManager.InvalidateEmailConfirmationTokenAsync(token);
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
}
