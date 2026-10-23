using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.UpdatePassword;

public class UpdatePasswordCommandHandler : ICommandHandler<UpdatePasswordCommand, Result>
{
  private readonly ILogger<UpdatePasswordCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ITokenService _tokenService;
  private readonly IBcryptService _bcryptService;

  public UpdatePasswordCommandHandler(
    ILogger<UpdatePasswordCommandHandler> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService,
    IBcryptService bcryptService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _tokenService = tokenService;
    _bcryptService = bcryptService;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
  {
    var validationResult = await _tokenService.ValidateTokenAsync(request.UpdatePasswordDto.Token, TokenType.PasswordReset);

    if (!validationResult.Data)
    {
      _logger.LogError("Invalid token provided for password update.");
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

    var email = _tokenService.GetEmailFromToken(request.UpdatePasswordDto.Token);

    if (email is null)
    {
      _logger.LogError("Token does not contain a valid email.");
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

    var user = await _userRepository.GetUserByEmailAsync(email, noTracking: false, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with email:{Email}", email);
      return Result.Failure(ApplicationError.UserNotFoundError(email: email));
    }

    var passwordHash = _bcryptService.Hash(request.UpdatePasswordDto.Password);

    user.UpdatePassword(passwordHash);

    user.InvalidateResetPasswordToken();

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Password updated successfully for user with email:{Email}", email);

    return Result.Success();

  }
}
