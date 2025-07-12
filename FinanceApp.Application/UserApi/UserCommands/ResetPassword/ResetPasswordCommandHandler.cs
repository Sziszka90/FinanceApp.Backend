using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.UserApi.UserCommands.ResetPassword;

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, Result>
{
  private readonly ILogger<ResetPasswordCommandHandler> _logger;
  private readonly IJwtService _jwtService;

  public ResetPasswordCommandHandler(
    ILogger<ResetPasswordCommandHandler> logger,
    IJwtService jwtService)
  {
    _logger = logger;
    _jwtService = jwtService;
  }

  public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
  {
    var validationResult = _jwtService.ValidateToken(request.Token);

    if (!validationResult)
    {
      _logger.LogError("Invalid token provided for password reset.");
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

    _jwtService.InvalidateToken(request.Token);
    _logger.LogDebug("Token invalidated successfully for password reset.");

    return Result.Success();
  }
}
