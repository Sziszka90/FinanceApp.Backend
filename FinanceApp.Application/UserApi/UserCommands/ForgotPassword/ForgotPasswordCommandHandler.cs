using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.UserApi.UserCommands.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, Result>
{
  private readonly ILogger<ForgotPasswordCommandHandler> _logger;
  private readonly ISmtpEmailSender _smtpEmailSender;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtService _jwtService;

  public ForgotPasswordCommandHandler(
    ILogger<ForgotPasswordCommandHandler> logger,
    ISmtpEmailSender smtpEmailSender,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IJwtService jwtService)
  {
    _logger = logger;
    _smtpEmailSender = smtpEmailSender;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _jwtService = jwtService;
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

    var resetPasswordToken = _jwtService.GenerateToken(request.EmailDto.Email);
    user.ResetPasswordToken = resetPasswordToken;
    user.ResetPasswordTokenExpiration = DateTimeOffset.UtcNow.AddHours(1);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Reset password token generated for user: {Email}", request.EmailDto.Email);

    await _smtpEmailSender.SendForgotPasswordAsync(request.EmailDto.Email, resetPasswordToken);

    _logger.LogInformation("Forgot password email sent to user: {Email}", request.EmailDto.Email);

    return Result.Success();
  }
}
