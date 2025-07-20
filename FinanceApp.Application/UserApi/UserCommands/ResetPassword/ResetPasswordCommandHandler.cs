using System.Data.Common;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.UserApi.UserCommands.ResetPassword;

public class ResetPasswordCommandHandler : ICommandHandler<ResetPasswordCommand, Result>
{
  private readonly ILogger<ResetPasswordCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtService _jwtService;

  public ResetPasswordCommandHandler(
    ILogger<ResetPasswordCommandHandler> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IJwtService jwtService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
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

    var email = _jwtService.GetUserEmailFromToken(request.Token);

    if(string.IsNullOrEmpty(email))
    {
      _logger.LogError("Email not found in token.");
      return Result.Failure(ApplicationError.EmailNotFoundInTokenError());
    }

    var user = await _userRepository.GetUserByEmailAsync(email, noTracking: false, cancellationToken);

    if (user is null)
    {
      _logger.LogWarning("User not found for email: {Email}", email);
      return Result.Failure(ApplicationError.UserNotFoundError(email));
    }

    if (user.ResetPasswordToken != request.Token)
    {
      _logger.LogWarning("Reset password token mismatch for user: {Email}", email);
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

    if (user.ResetPasswordTokenExpiration < DateTime.UtcNow)
    {
      _logger.LogWarning("Reset password token expired for user: {Email}", email);
      return Result.Failure(ApplicationError.ResetPasswordTokenExpiredError(email));
    }

    _jwtService.InvalidateToken(request.Token);
    _logger.LogDebug("Token invalidated successfully for password reset.");

    user.ResetPasswordToken = null;
    user.ResetPasswordTokenExpiration = null;

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    _logger.LogInformation("Password reset successfully for user: {Email}", email);

    return Result.Success();
  }
}
