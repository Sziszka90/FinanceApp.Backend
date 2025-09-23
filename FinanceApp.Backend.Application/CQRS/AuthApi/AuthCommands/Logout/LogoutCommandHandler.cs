using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.Logout;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, Result>
{
  private readonly ILogger<LogoutCommandHandler> _logger;
  private readonly IUserService _userService;
  private readonly ITokenService _tokenService;

  public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger,
                             IUserService userService,
                             ITokenService tokenService)
  {
    _logger = logger;
    _userService = userService;
    _tokenService = tokenService;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
  {
    var tokenResult = _userService.GetActiveUserToken();
    var refreshTokenResult = _userService.GetActiveUserRefreshToken();

    if (!tokenResult.IsSuccess || string.IsNullOrEmpty(tokenResult.Data))
    {
      _logger.LogInformation("User not logged in");
    }
    else
    {
      await _tokenService.InvalidateTokenAsync(tokenResult.Data!, TokenType.Login);
    }

    if (!refreshTokenResult.IsSuccess || string.IsNullOrEmpty(refreshTokenResult.Data))
    {
      _logger.LogInformation("Refresh token not found for user logout");
    }
    else
    {
      await _tokenService.InvalidateRefreshTokenAsync(refreshTokenResult.Data!);
    }

    _logger.LogInformation("Login and refresh token invalidated successfully!");

    return Result.Success();
  }
}
