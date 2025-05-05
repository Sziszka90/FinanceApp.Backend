using AutoMapper;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.User.UserCommands;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<GetUserDto>>
{
  #region Members

  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly ILogger<CreateUserCommandHandler> _logger;

  #endregion

  #region Constructors

  public CreateUserCommandHandler(IMapper mapper,
                                  IUnitOfWork unitOfWork,
                                  IRepository<Domain.Entities.User> userRepository,
                                  ILogger<CreateUserCommandHandler> logger)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _userRepository = userRepository;
    _logger = logger;
  }

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<Result<GetUserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
  {
    var criteria = UserQueryCriteria.FindUserName(request.CreateUserDto);

    var usersWithSameName = await _userRepository.GetQueryAsync(criteria, cancellationToken: cancellationToken);

    if (usersWithSameName.Count > 0)
    {
      _logger.LogError("User already exists with name:{Name}", request.CreateUserDto.UserName);
      return Result.Failure<GetUserDto>(ApplicationError.UserNameAlreadyExistsError(request.CreateUserDto.UserName));
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.CreateUserDto.Password);

    var user = await _userRepository.CreateAsync(new Domain.Entities.User(
                                                   request.CreateUserDto.UserName,
                                                   passwordHash,
                                                   request.CreateUserDto.BaseCurrency), cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("User created with ID:{Id}", user.Id);

    return Result.Success(_mapper.Map<GetUserDto>(user));
  }

  #endregion
}