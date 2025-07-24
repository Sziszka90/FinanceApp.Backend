using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ConfirmUserEmail;

public class ConfirmUserEmailCommandHandler : ICommandHandler<ConfirmUserEmailCommand, Result>
{
  private readonly ILogger<ConfirmUserEmailCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IJwtService _jwtService;
  private readonly ICacheManager _cacheManager;

  public ConfirmUserEmailCommandHandler(
    ILogger<ConfirmUserEmailCommandHandler> logger,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IJwtService jwtService,
    ICacheManager cacheManager)
  {
    _logger = logger;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _jwtService = jwtService;
    _cacheManager = cacheManager;
  }

  public async Task<Result> Handle(ConfirmUserEmailCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(request.Id, noTracking: false, cancellationToken);

    if (user is null)
    {
       _logger.LogError("User not found with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.UserNotFoundError());
    }

    var validationResult = _jwtService.ValidateToken(request.Token);
    _jwtService.InvalidateToken(request.Token);

    if (!validationResult)
    {
      _logger.LogError("Invalid token for user with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EmailConfirmationError(user.Email));
    }

    if (user.EmailConfirmationToken != request.Token)
    {
      _logger.LogError("Invalid token for user with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EmailConfirmationError(user.Email));
    }

    if(user.EmailConfirmationTokenExpiration < DateTimeOffset.UtcNow)
    {
      _logger.LogError("Token expired for user with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.TokenExpiredError(user.Email));
    }

    var validateTokenCache = await _cacheManager.IsEmailConfirmationTokenValidAsync(request.Token);
    await _cacheManager.InvalidateEmailConfirmationTokenAsync(request.Token);

    if (!validateTokenCache)
    {
      _logger.LogError("Invalid token for user with ID:{Id}", request.Id);
      return Result.Failure(ApplicationError.EmailConfirmationError(user.Email));
    }

    _logger.LogDebug("Email confirmation token validated for user with ID:{Id}", request.Id);

    user.EmailConfirmationToken = null;
    user.EmailConfirmationTokenExpiration = null;
    user.IsEmailConfirmed = true;

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogDebug("Email confirmed for user with ID:{Id}", request.Id);

    return Result.Success();
  }
}
