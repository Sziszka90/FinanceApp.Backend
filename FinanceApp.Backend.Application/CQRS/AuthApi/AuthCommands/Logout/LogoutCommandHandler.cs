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
    var token = _userService.GetActiveUserToken();

    if (!token.IsSuccess)
    {
      _logger.LogWarning("User not logged in");
      return Result.Failure<LoginResponseDto>(token.ApplicationError!);
    }

    await _tokenService.InvalidateTokenAsync(token.Data!, TokenType.Login);

    _logger.LogInformation("Login token invalidated successful!");

    return Result.Success();
  }
}
