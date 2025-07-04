using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.User.UserCommands;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, Result<GetUserDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly ILogger<UpdateUserCommandHandler> _logger;
  public UpdateUserCommandHandler(IMapper mapper,
                                  IUnitOfWork unitOfWork,
                                  IRepository<Domain.Entities.User> userRepository,
                                  ILogger<UpdateUserCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _userRepository = userRepository;
    _logger = logger;
  }

  /// <inheritdoc />
  public async Task<Result<GetUserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetByIdAsync(request.UpdateUserDto.Id, cancellationToken);

    if (user is null)
    {
      _logger.LogError("User not found with ID:{Id}", request.UpdateUserDto.Id);
      return Result.Failure<GetUserDto>(ApplicationError.EntityNotFoundError());
    }

    var criteriaForUserName = UserQueryCriteria.FindUserName(request.UpdateUserDto.UserName);

    var usersWithSameName = await _userRepository.GetQueryAsync(criteriaForUserName, cancellationToken: cancellationToken);

    if (usersWithSameName.Count > 0 &&
        usersWithSameName[0].Id != user.Id)
    {
      _logger.LogError("User already exists with name:{Name}", request.UpdateUserDto.UserName);
      return Result.Failure<GetUserDto>(ApplicationError.UserNameAlreadyExistsError(request.UpdateUserDto.UserName));
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.UpdateUserDto.Password);

    user.Update(request.UpdateUserDto.UserName, passwordHash,  request.UpdateUserDto.BaseCurrency);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("User updated with ID:{Id}", user.Id);

    return Result.Success(_mapper.Map<GetUserDto>(user));
  }
}
