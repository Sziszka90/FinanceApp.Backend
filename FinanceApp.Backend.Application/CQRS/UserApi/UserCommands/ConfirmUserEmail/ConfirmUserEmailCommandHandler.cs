using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ConfirmUserEmail;

public class ConfirmUserEmailCommandHandler : ICommandHandler<ConfirmUserEmailCommand, Result>
{
  private readonly ILogger<ConfirmUserEmailCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ITokenService _tokenService;

  public ConfirmUserEmailCommandHandler(
    ILogger<ConfirmUserEmailCommandHandler> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ITokenService tokenService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _tokenService = tokenService;
  }

  public async Task<Result> Handle(ConfirmUserEmailCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(request.Id, noTracking: false, cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.UserNotFoundError(userId: request.Id.ToString()));
    }

    if (user.EmailConfirmationTokenExpiration < DateTimeOffset.UtcNow)
    {
      _logger.LogError("Token expired for user with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.TokenExpiredError(user.Email));
    }

    var result = await _tokenService.ValidateTokenAsync(request.Token, TokenType.EmailConfirmation);

    if (!result.Data)
    {
      _logger.LogError("Invalid token provided for user with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.InvalidTokenError(user.Email));
    }

    _logger.LogInformation("Email confirmation token validated for user with ID:{Id}", request.Id);

    user.InvalidateEmailConfirmationToken();
    user.ConfirmEmail();

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Email confirmed for user with ID:{Id}", request.Id);

    return Result.Success();
  }
}
