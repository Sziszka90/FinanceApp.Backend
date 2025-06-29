using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstraction.Services;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.User.UserCommands;

public class UpdatePasswordCommandHandler : ICommandHandler<UpdatePasswordCommand, Result>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserRepository _userRepository;
  private readonly ILogger<UpdatePasswordCommandHandler> _logger;
  private IJwtService _jwtService;
  public UpdatePasswordCommandHandler(IMapper mapper,
                                  IUnitOfWork unitOfWork,
                                  IUserRepository userRepository,
                                  ILogger<UpdatePasswordCommandHandler> logger,
                                  IJwtService jwtService)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _userRepository = userRepository;
    _logger = logger;
    _jwtService = jwtService;
  }

  /// <inheritdoc />
  public async Task<Result> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
  {
    if (request.UpdatePasswordDto.Token is null)
    {
      return Result.Failure(ApplicationError.InvalidTokenError());
    }

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

    var user = await _userRepository.GetUserByEmailAsync(email, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with email:{Email}", email);
      return Result.Failure(ApplicationError.UserNotFoundError());
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.UpdatePasswordDto.Password);

    user.UpdatePassword(passwordHash);

    await _userRepository.UpdateAsync(user);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _jwtService.InvalidateToken(request.UpdatePasswordDto.Token);

    return Result.Success();

  }
}
