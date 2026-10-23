using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace FinanceApp.Backend.Application.AuthApi.AuthQueries.CheckQuery;

public class CheckQueryHandler : IQueryHandler<CheckQuery, Result<bool>>
{
  private readonly ILogger<CheckQueryHandler> _logger;
  private readonly ITokenService _tokenService;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CheckQueryHandler(
    ILogger<CheckQueryHandler> logger,
    ITokenService tokenService,
    IHttpContextAccessor httpContextAccessor
  )
  {
    _logger = logger;
    _tokenService = tokenService;
    _httpContextAccessor = httpContextAccessor;
  }

  public async Task<Result<bool>> Handle(CheckQuery request, CancellationToken cancellationToken)
  {
    var context = _httpContextAccessor.HttpContext;

    if (context is null)
    {
      _logger.LogInformation("HttpContext is null");
      return Result.Success(false);
    }

    var token = context.Request.Cookies["Token"];
    var refreshToken = context.Request.Cookies["RefreshToken"];

    if (!string.IsNullOrEmpty(token))
    {
      var tokenValidationResult = await _tokenService.IsTokenValidAsync(token, TokenType.Login, cancellationToken);

      if (!tokenValidationResult.IsSuccess)
      {
        _logger.LogWarning("Token validation failed: {Error}", tokenValidationResult.ApplicationError?.Message);
        return Result.Failure<bool>(tokenValidationResult.ApplicationError!);
      }

      if (tokenValidationResult.Data)
      {
        _logger.LogInformation("Login Token is valid");
        return Result.Success(true);
      }
    }

    if (!string.IsNullOrEmpty(refreshToken))
    {
      var refreshTokenValidationResult = await _tokenService.IsRefreshTokenValidAsync(refreshToken, cancellationToken);

      if (!refreshTokenValidationResult.IsSuccess)
      {
        _logger.LogWarning("Token validation failed: {Error}", refreshTokenValidationResult.ApplicationError?.Message);
        return Result.Failure<bool>(refreshTokenValidationResult.ApplicationError!);
      }

      if (refreshTokenValidationResult.Data)
      {
        _logger.LogInformation("Refresh Token is valid");
        return Result.Success(true);
      }
    }

    _logger.LogInformation("Token and RefreshToken cookies not found or empty");
    return Result.Success(false);
  }
}
