using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginResponseDto>>
{
  private readonly ILogger<LoginCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IJwtService _jwtService;
  private readonly IBcryptService _bcryptService;
  private readonly ICacheManager _cacheManager;

  public LoginCommandHandler(ILogger<LoginCommandHandler> logger,
                             IUserRepository userRepository,
                             IJwtService jwtService,
                             IBcryptService bcryptService,
                             ICacheManager cacheManager)
  {
    _logger = logger;
    _userRepository = userRepository;
    _jwtService = jwtService;
    _bcryptService = bcryptService;
    _cacheManager = cacheManager;
  }

  /// <inheritdoc />
  public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetUserByEmailAsync(request.LoginRequestDto.Email, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogWarning("User not found with email:{Email}", request.LoginRequestDto.Email);
      return Result.Failure<LoginResponseDto>(ApplicationError.UserNotFoundError());
    }

    if(!user.IsEmailConfirmed)
    {
      _logger.LogWarning("User with email:{Email} has not confirmed email", request.LoginRequestDto.Email);
      return Result.Failure<LoginResponseDto>(ApplicationError.EmailNotYetConfirmedError(user.Email));
    }

    if (!_bcryptService.Verify(request.LoginRequestDto.Password, user.PasswordHash))
    {
      _logger.LogWarning("Invalid password for user:{Email}", request.LoginRequestDto.Email);
      return Result.Failure<LoginResponseDto>(ApplicationError.InvalidPasswordError());
    }

    var token = _jwtService.GenerateToken(request.LoginRequestDto.Email);

    await _cacheManager.SaveTokenAsync(token);

    _logger.LogDebug("Login successful!");

    var result = await _cacheManager.IsTokenInvalidAsync(token);

    if (result)
    {
      _logger.LogDebug("Token already exists in cache, returning existing token.");
      return Result.Success(new LoginResponseDto
      {
        Token = token
      });
    }

    return Result.Success(new LoginResponseDto
    {
      Token = token
    });
  }
}
