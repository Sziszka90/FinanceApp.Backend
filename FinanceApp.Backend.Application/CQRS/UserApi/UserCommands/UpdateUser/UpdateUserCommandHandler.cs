using System.Security.Claims;
using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.UpdateUser;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, Result<GetUserDto>>
{
  private readonly ILogger<UpdateUserCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IHttpContextAccessor _httpContextAccessor;
  private readonly IBcryptService _bcryptService;
  public UpdateUserCommandHandler(
    ILogger<UpdateUserCommandHandler> logger,
    IMapper mapper,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor,
    IBcryptService bcryptService)
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _httpContextAccessor = httpContextAccessor;
    _bcryptService = bcryptService;
  }

  /// <inheritdoc />
  public async Task<Result<GetUserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (string.IsNullOrEmpty(userEmail))
    {
      _logger.LogError("User email not found in claims.");
      return Result.Failure<GetUserDto>(ApplicationError.UserNotFoundError());
    }

    var user = await _userRepository.GetUserByEmailAsync(userEmail, noTracking: false, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with ID:{Id}", request.UpdateUserDto.Id);
      return Result.Failure<GetUserDto>(ApplicationError.EntityNotFoundError());
    }

    if (!string.IsNullOrWhiteSpace(request.UpdateUserDto.UserName))
    {
      var criteriaForUserName = UserQueryCriteria.FindUserName(request.UpdateUserDto.UserName);

      var usersWithSameName = await _userRepository.GetQueryAsync(criteriaForUserName, noTracking: true, cancellationToken: cancellationToken);

      if (usersWithSameName is not null &&
          usersWithSameName.Count > 0 &&
          usersWithSameName[0].Id != user.Id)
      {
        _logger.LogError("User already exists with name:{Name}", request.UpdateUserDto.UserName);
        return Result.Failure<GetUserDto>(ApplicationError.UserNameAlreadyExistsError(request.UpdateUserDto.UserName));
      }
    }

    string? passwordHash = null;

    if (!string.IsNullOrWhiteSpace(request.UpdateUserDto.Password))
    {
      passwordHash = _bcryptService.Hash(request.UpdateUserDto.Password);
    }

    user.Update(request.UpdateUserDto.UserName, passwordHash, request.UpdateUserDto.BaseCurrency);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("User updated with ID:{Id}", user.Id);

    return Result.Success(_mapper.Map<GetUserDto>(user));
  }
}
