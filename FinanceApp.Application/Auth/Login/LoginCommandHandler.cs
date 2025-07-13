using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.AuthDtos;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.AuthApi.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginResponseDto>>
{
  private readonly ILogger<LoginCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IJwtService _jwtService;
  private readonly IBcryptService _bcryptService;

  public LoginCommandHandler(ILogger<LoginCommandHandler> logger,
                             IUserRepository userRepository,
                             IJwtService jwtService,
                             IBcryptService bcryptService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _jwtService = jwtService;
    _bcryptService = bcryptService;
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

    _logger.LogDebug("Login successful!");

    return Result.Success(new LoginResponseDto
    {
      Token = token
    });
  }
}
