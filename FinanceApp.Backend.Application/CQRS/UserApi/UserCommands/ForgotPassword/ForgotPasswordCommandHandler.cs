using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, Result>
{
  private readonly ILogger<ForgotPasswordCommandHandler> _logger;
  private readonly ISmtpEmailSender _smtpEmailSender;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ITokenService _tokenService;

  public ForgotPasswordCommandHandler(
    ILogger<ForgotPasswordCommandHandler> logger,
    ISmtpEmailSender smtpEmailSender,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService)
  {
    _logger = logger;
    _smtpEmailSender = smtpEmailSender;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _tokenService = tokenService;
  }

  public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetUserByEmailAsync(request.EmailDto.Email, noTracking: false, cancellationToken);

    if (user is null)
    {
      _logger.LogWarning("User not found: {Email}", request.EmailDto.Email);
      return Result.Failure(ApplicationError.UserNotFoundError(request.EmailDto.Email));
    }

    if(!user.IsEmailConfirmed)
    {
      _logger.LogWarning("Email not confirmed for user: {Email}", request.EmailDto.Email);
      return Result.Failure(ApplicationError.EmailConfirmationError(user.Email));
    }

    var resetPasswordToken = await _tokenService.GenerateTokenAsync(request.EmailDto.Email, TokenType.PasswordReset);

    if (!resetPasswordToken.IsSuccess)
    {
      _logger.LogError("Failed to generate password reset token for user: {Email}", request.EmailDto.Email);
      return Result.Failure(resetPasswordToken.ApplicationError!);
    }

    user.ResetPasswordToken = resetPasswordToken.Data;
    user.ResetPasswordTokenExpiration = DateTimeOffset.UtcNow.AddHours(24);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Reset password token generated for user: {Email}", request.EmailDto.Email);

    await _smtpEmailSender.SendForgotPasswordAsync(request.EmailDto.Email, resetPasswordToken.Data!);

    _logger.LogInformation("Forgot password email sent to user: {Email}", request.EmailDto.Email);

    return Result.Success();
  }
}
