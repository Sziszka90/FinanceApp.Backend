using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.Refresh;

public class RefreshCommandHandler : ICommandHandler<RefreshCommand, Result<string>>
{
  private readonly ILogger<RefreshCommandHandler> _logger;
  private readonly ITokenService _tokenService;

  public RefreshCommandHandler(
    ILogger<RefreshCommandHandler> logger,
    ITokenService tokenService)
  {
    _logger = logger;
    _tokenService = tokenService;
  }

  public async Task<Result<string>> Handle(RefreshCommand request, CancellationToken cancellationToken)
  {
    var isValid = await _tokenService.IsRefreshTokenValidAsync(request.RefreshToken);

    if (!isValid.Data)
    {
      _logger.LogWarning("Invalid token provided: {Token}", request.RefreshToken);
      return Result.Failure<string>(ApplicationError.InvalidTokenError("RefreshToken"));
    }

    var email = _tokenService.GetEmailFromToken(request.RefreshToken);
    if (string.IsNullOrEmpty(email))
    {
      _logger.LogWarning("No active user found for token refresh.");
      return Result.Failure<string>(ApplicationError.UserNotFoundError("Unknown"));
    }

    var token = await _tokenService.GenerateTokenAsync(email, TokenType.Login);

    _logger.LogInformation("Token validated successfully: {Token}", request.RefreshToken);

    return Result.Success(token.Data!);
  }
}
