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
  #region Members

  private readonly IMapper _mapper;
  private readonly ILogger<LoginCommandHandler> _logger;
  private readonly IUserRepository _userRepository;
  private readonly IJwtService _jwtService;

  #endregion

  #region Constructors

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

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByUserNameAsync(request.LoginRequestDto.UserName, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with name:{Name}", request.LoginRequestDto.UserName);
      return Result.Failure<LoginResponseDto>(ApplicationError.UserNotFoundError());
    }

    if (!BCrypt.Net.BCrypt.Verify(request.LoginRequestDto.Password, user.PasswordHash))
    {
      _logger.LogError("Invalid password for user:{Name}", request.LoginRequestDto.UserName);
      return Result.Failure<LoginResponseDto>(ApplicationError.InvalidPasswordError());
    }

    var token = _jwtService.GenerateToken(request.LoginRequestDto.UserName);

    _logger.LogInformation("Login successful!");

    return Result.Success(new LoginResponseDto
    {
      Token = token
    });
  }

  #endregion
}