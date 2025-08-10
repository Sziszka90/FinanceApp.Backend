using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.UpdateUser;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, Result<GetUserDto>>
{
  private readonly ILogger<UpdateUserCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IUserRepository _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserService _userService;
  private readonly IBcryptService _bcryptService;
  public UpdateUserCommandHandler(
    ILogger<UpdateUserCommandHandler> logger,
    IMapper mapper,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IUserService userService,
    IBcryptService bcryptService)
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _userService = userService  ;
    _bcryptService = bcryptService;
  }

  /// <inheritdoc />
  public async Task<Result<GetUserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return Result.Failure<GetUserDto>(user.ApplicationError!);
    }

    if (!string.IsNullOrWhiteSpace(request.UpdateUserDto.UserName))
    {
      var criteriaForUserName = UserQueryCriteria.FindUserName(request.UpdateUserDto.UserName);

      var usersWithSameName = await _userRepository.GetQueryAsync(criteriaForUserName, noTracking: true, cancellationToken: cancellationToken);

      if (usersWithSameName is not null &&
          usersWithSameName.Count > 0 &&
          usersWithSameName[0].Id != user.Data!.Id)
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

    user.Data!.Update(request.UpdateUserDto.UserName, passwordHash, request.UpdateUserDto.BaseCurrency);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("User updated with ID:{Id}", user.Data!.Id);

    return Result.Success(_mapper.Map<GetUserDto>(user));
  }
}
