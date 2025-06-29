using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.AuthDtos;
using FinanceApp.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.Auth;

public class LoginCommandHandler : ICommandHandler<LoginCommand, Result<LoginResponseDto>>
{
  private readonly IMapper _mapper;
  private readonly ILogger<LoginCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IJwtService _jwtService;

  public LoginCommandHandler(IMapper mapper,
                             ILogger<LoginCommandHandler> logger,
                             IUserRepository userRepository,
                             IJwtService jwtService)
  {
    _mapper = mapper;
    _logger = logger;
    _userRepository = userRepository;
    _jwtService = jwtService;
  }

  /// <inheritdoc />
  public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetUserByEmailAsync(request.LoginRequestDto.Email, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with email:{Email}", request.LoginRequestDto.Email);
      return Result.Failure<LoginResponseDto>(ApplicationError.UserNotFoundError());
    }

    if(!user.IsEmailConfirmed)
    {
      _logger.LogError("User with email:{Email} has not confirmed email", request.LoginRequestDto.Email);
      return Result.Failure<LoginResponseDto>(ApplicationError.EmailNotYetConfirmedError(user.Email));
    }

    if (!BCrypt.Net.BCrypt.Verify(request.LoginRequestDto.Password, user.PasswordHash))
    {
      _logger.LogError("Invalid password for user:{Email}", request.LoginRequestDto.Email);
      return Result.Failure<LoginResponseDto>(ApplicationError.InvalidPasswordError());
    }

    var token = _jwtService.GenerateToken(request.LoginRequestDto.Email);

    _logger.LogInformation("Login successful!");

    return Result.Success(new LoginResponseDto
    {
      Token = token
    });
  }
}
