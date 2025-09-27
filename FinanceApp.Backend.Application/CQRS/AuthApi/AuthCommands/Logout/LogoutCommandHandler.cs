using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.AuthApi.AuthCommands.ResetToken;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.Logout;

public class LogoutCommandHandler : ICommandHandler<LogoutCommand, Result>
{
  private readonly ILogger<LogoutCommandHandler> _logger;
  private readonly IUserService _userService;
  private readonly ITokenService _tokenService;
  private readonly IMediator _mediator;

  public LogoutCommandHandler(ILogger<LogoutCommandHandler> logger,
                             IUserService userService,
                             ITokenService tokenService,
                             IMediator mediator)
  {
    _logger = logger;
    _userService = userService;
    _tokenService = tokenService;
    _mediator = mediator;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
  {
    if (!string.IsNullOrEmpty(request.Token))
    {
      await _tokenService.InvalidateTokenAsync(request.Token!, TokenType.Login);
      _logger.LogInformation("Token invalidated successfully!");
    }

    if (!string.IsNullOrEmpty(request.RefreshToken))
    {
      await _tokenService.InvalidateRefreshTokenAsync(request.RefreshToken);
      _logger.LogInformation("Refresh token invalidated successfully!");
    }

    return Result.Success();
  }
}
