using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Application.UserApi.UserCommands.ValidateToken;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;

public class ValidateTokenCommandHandler : ICommandHandler<ValidateTokenCommand, Result<ValidateTokenResponse>>
{
  private readonly ILogger<ValidateTokenCommandHandler> _logger;
  private readonly ITokenService _tokenService;

  public ValidateTokenCommandHandler(
    ILogger<ValidateTokenCommandHandler> logger,
    ITokenService tokenService)
  {
    _logger = logger;
    _tokenService = tokenService;
  }

  public async Task<Result<ValidateTokenResponse>> Handle(ValidateTokenCommand request, CancellationToken cancellationToken)
  {
    var isValid = await _tokenService.IsTokenValidAsync(request.validateTokenRequest.Token, request.validateTokenRequest.TokenType);

    if (!isValid)
    {
      _logger.LogWarning("Invalid token provided: {Token}", request.validateTokenRequest.Token);
      return Result.Success(new ValidateTokenResponse { IsValid = false });
    }

    _logger.LogInformation("Token validated successfully: {Token}", request.validateTokenRequest.Token);
    return Result.Success(new ValidateTokenResponse { IsValid = true });
  }
}
