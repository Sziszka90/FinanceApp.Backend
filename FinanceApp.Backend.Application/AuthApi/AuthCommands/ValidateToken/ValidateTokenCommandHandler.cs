using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.UserApi.UserCommands.ValidateToken;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;

public class ValidateTokenCommandHandler : ICommandHandler<ValidateTokenCommand, Result<ValidateTokenResponse>>
{
  private readonly ILogger<ValidateTokenCommandHandler> _logger;
  private readonly IJwtService _jwtService;

  public ValidateTokenCommandHandler(
    ILogger<ValidateTokenCommandHandler> logger,
    IJwtService jwtService)
  {
    _logger = logger;
    _jwtService = jwtService;
  }

  public Task<Result<ValidateTokenResponse>> Handle(ValidateTokenCommand request, CancellationToken cancellationToken)
  {
    var isValid = _jwtService.ValidateToken(request.Token);

    if (!isValid)
    {
      _logger.LogWarning("Invalid token provided: {Token}", request.Token);
      return Task.FromResult(Result.Success(new ValidateTokenResponse { IsValid = false }));
    }

    _logger.LogInformation("Token validated successfully: {Token}", request.Token);
    return Task.FromResult(Result.Success(new ValidateTokenResponse { IsValid = true }));
  }
}
