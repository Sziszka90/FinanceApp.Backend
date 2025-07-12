using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using FinanceApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.UserApi.UserCommands.UpdateUser;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, Result<GetUserDto>>
{
  private readonly ILogger<UpdateUserCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly IUnitOfWork _unitOfWork;
  public UpdateUserCommandHandler(
    ILogger<UpdateUserCommandHandler> logger,
    IMapper mapper,
    IRepository<User> userRepository,
    IUnitOfWork unitOfWork)
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
  }

  /// <inheritdoc />
  public async Task<Result<GetUserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(request.UpdateUserDto.Id, noTracking: false, cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with ID:{Id}", request.UpdateUserDto.Id);
      return Result.Failure<GetUserDto>(ApplicationError.EntityNotFoundError());
    }

    var criteriaForUserName = UserQueryCriteria.FindUserName(request.UpdateUserDto.UserName);

    var usersWithSameName = await _userRepository.GetQueryAsync(criteriaForUserName, noTracking: true, cancellationToken: cancellationToken);

    if (usersWithSameName.Count > 0 &&
        usersWithSameName[0].Id != user.Id)
    {
      _logger.LogError("User already exists with name:{Name}", request.UpdateUserDto.UserName);
      return Result.Failure<GetUserDto>(ApplicationError.UserNameAlreadyExistsError(request.UpdateUserDto.UserName));
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.UpdateUserDto.Password);

    user.Update(request.UpdateUserDto.UserName, passwordHash,  request.UpdateUserDto.BaseCurrency);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogDebug("User updated with ID:{Id}", user.Id);

    return Result.Success(_mapper.Map<GetUserDto>(user));
  }
}
