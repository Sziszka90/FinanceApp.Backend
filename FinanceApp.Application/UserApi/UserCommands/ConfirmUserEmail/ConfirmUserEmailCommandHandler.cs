using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.UserApi.UserCommands.ConfirmUserEmail;

public class ConfirmUserEmailCommandHandler : ICommandHandler<ConfirmUserEmailCommand, Result>
{
  private readonly ILogger<ConfirmUserEmailCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtService _jwtService;

  public ConfirmUserEmailCommandHandler(
    ILogger<ConfirmUserEmailCommandHandler> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IJwtService jwtService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _jwtService = jwtService;
  }

  public async Task<Result> Handle(ConfirmUserEmailCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(request.Id, noTracking: true, cancellationToken);

    if (user is null)
    {
       _logger.LogError("User not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.UserNotFoundError());
    }

    var validationResult = _jwtService.ValidateToken(request.Token);

    if (user.EmailConfirmationToken != request.Token)
    {
      _logger.LogError("Invalid token for user with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EmailConfirmationError(user.Email));
    }

    if(user.EmailConfirmationTokenExpiration < DateTime.UtcNow)
    {
      _logger.LogError("Token expired for user with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.TokenExpiredError(user.Email));
    }

    if (!validationResult)
    {
      _logger.LogError("Invalid token for user with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EmailConfirmationError(user.Email));
    }

    user.EmailConfirmationToken = null;
    user.EmailConfirmationTokenExpiration = null;
    user.IsEmailConfirmed = true;

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _jwtService.InvalidateToken(request.Token);

    _logger.LogDebug("Email confirmed for user with ID:{Id}", request.Id);

    return Result.Success();
  }
}
