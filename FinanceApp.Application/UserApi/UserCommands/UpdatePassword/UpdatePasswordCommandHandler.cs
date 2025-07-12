using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.UserApi.UserCommands.UpdatePassword;

public class UpdatePasswordCommandHandler : ICommandHandler<UpdatePasswordCommand, Result>
{
  private readonly ILogger<UpdatePasswordCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtService _jwtService;

  public UpdatePasswordCommandHandler(
    ILogger<UpdatePasswordCommandHandler> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IJwtService jwtService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _jwtService = jwtService;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
  {
    var validationResult = _jwtService.ValidateToken(request.UpdatePasswordDto.Token);
    if (!validationResult)
    {
      _logger.LogError("Invalid token provided for password update.");
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

    var email = _jwtService.GetUserEmailFromToken(request.UpdatePasswordDto.Token);

    if (email is null)
    {
      _logger.LogError("Token does not contain a valid email.");
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

    var user = await _userRepository.GetUserByEmailAsync(email, noTracking: false, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with email:{Email}", email);
      return Result.Failure(ApplicationError.UserNotFoundError());
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.UpdatePasswordDto.Password);

    user.UpdatePassword(passwordHash);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogDebug("Password updated successfully for user with email:{Email}", email);

    _jwtService.InvalidateToken(request.UpdatePasswordDto.Token);

    _logger.LogDebug("Token invalidated successfully after password update for user with email:{Email}", email);

    return Result.Success();

  }
}
