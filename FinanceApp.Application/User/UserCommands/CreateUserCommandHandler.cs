using AutoMapper;
using FinanceApp.Application.Abstraction.Clients;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FinanceApp.Application.QueryCriteria;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.User.UserCommands;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<GetUserDto>>
{
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly ILogger<CreateUserCommandHandler> _logger;
  private readonly ISmtpEmailSender _smtpEmailSender;

  public CreateUserCommandHandler(IMapper mapper,
                                  IUnitOfWork unitOfWork,
                                  IRepository<Domain.Entities.User> userRepository,
                                  ILogger<CreateUserCommandHandler> logger,
                                  ISmtpEmailSender smtpEmailSender)
  {
    _mapper = mapper;
    _unitOfWork = unitOfWork;
    _userRepository = userRepository;
    _logger = logger;
    _smtpEmailSender = smtpEmailSender;
  }

  /// <inheritdoc />
  public async Task<Result<GetUserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
  {
    var criteriaForUserName = UserQueryCriteria.FindUserName(request.CreateUserDto);

    var usersWithSameName = await _userRepository.GetQueryAsync(criteriaForUserName, cancellationToken: cancellationToken);

    if (usersWithSameName.Count > 0)
    {
      _logger.LogError("User already exists with name:{Name}", request.CreateUserDto.UserName);
      return Result.Failure<GetUserDto>(ApplicationError.UserNameAlreadyExistsError(request.CreateUserDto.UserName));
    }

    var criteriaForEmail = UserQueryCriteria.FindUserEmail(request.CreateUserDto);

    var userWithSameEmail = await _userRepository.GetQueryAsync(criteriaForEmail, cancellationToken: cancellationToken);

    if (userWithSameEmail.Count > 0)
    {
      _logger.LogError("User already exists with email:{Email}", request.CreateUserDto.Email);
      return Result.Failure<GetUserDto>(ApplicationError.UserEmailAlreadyExistsError(request.CreateUserDto.Email));
    }

    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.CreateUserDto.Password);

    var user = await _userRepository.CreateAsync(new Domain.Entities.User(
                                                   request.CreateUserDto.UserName,
                                                   request.CreateUserDto.Email,
                                                   passwordHash,
                                                   request.CreateUserDto.BaseCurrency), cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    await _smtpEmailSender.SendEmailConfirmationAsync(user);

    _logger.LogInformation("User created with ID:{Id}", user.Id);

    return Result.Success(_mapper.Map<GetUserDto>(user));
  }
}
