using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.AuthDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginResponseDto>>
{
  private readonly ILogger<LoginCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly ITokenService _tokenService;

  public LoginCommandHandler(ILogger<LoginCommandHandler> logger,
                             IUserRepository userRepository,
                             ITokenService tokenService)
  {
    _logger = logger;
    _userRepository = userRepository;
    _tokenService = tokenService;
  }

  /// <inheritdoc />
  public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetUserByEmailAsync(request.LoginRequestDto.Email, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogWarning("User not found with email:{Email}", request.LoginRequestDto.Email);
      return Result.Failure<LoginResponseDto>(ApplicationError.UserNotFoundError(email: request.LoginRequestDto.Email));
    }

    if(!user.IsEmailConfirmed)
    {
      _logger.LogWarning("User with email:{Email} has not confirmed email", request.LoginRequestDto.Email);
      return Result.Failure<LoginResponseDto>(ApplicationError.EmailNotYetConfirmedError(user.Email));
    }

    var token = await _tokenService.GenerateTokenAsync(request.LoginRequestDto.Email, TokenType.Login);

    if (!token.IsSuccess)
    {
      _logger.LogError("Failed to generate token for user with email:{Email}. Error: {Error}", request.LoginRequestDto.Email, token.ApplicationError?.Message);
      return Result.Failure<LoginResponseDto>(token.ApplicationError!);
    }

    _logger.LogInformation("Login successful!");

    return Result.Success(new LoginResponseDto
    {
      Token = token.Data!
    });
  }
}
