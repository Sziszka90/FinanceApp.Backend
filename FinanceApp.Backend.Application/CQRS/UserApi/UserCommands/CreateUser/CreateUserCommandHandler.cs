using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Domain.Icons;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Result<GetUserDto>>
{
  private readonly ILogger<CreateUserCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IRepository<Domain.Entities.User> _userRepository;
  private readonly ITransactionGroupRepository _transactionGroupRepository;
  private readonly ISmtpEmailSender _smtpEmailSender;
  private readonly IBcryptService _bcryptService;
  private readonly ITokenService _tokenService;

  public CreateUserCommandHandler(
    ILogger<CreateUserCommandHandler> logger,
    IMapper mapper,
    IRepository<Domain.Entities.User> userRepository,
    ITransactionGroupRepository transactionGroupRepository,
    IUnitOfWork unitOfWork,
    ISmtpEmailSender smtpEmailSender,
    IBcryptService bcryptService,
    ITokenService tokenService)
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
    _smtpEmailSender = smtpEmailSender;
    _bcryptService = bcryptService;
    _tokenService = tokenService;
  }

  /// <inheritdoc />
  public async Task<Result<GetUserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
  {
    var criteriaForUserName = UserQueryCriteria.FindUserName(request.CreateUserDto);

    var usersWithSameName = await _userRepository.GetQueryAsync(criteriaForUserName, noTracking: true, cancellationToken: cancellationToken);

    if (usersWithSameName.Count > 0)
    {
      _logger.LogWarning("User already exists with name:{Name}", request.CreateUserDto.UserName);
      return Result.Failure<GetUserDto>(ApplicationError.UserNameAlreadyExistsError(request.CreateUserDto.UserName));
    }

    var criteriaForEmail = UserQueryCriteria.FindUserEmail(request.CreateUserDto);

    var userWithSameEmail = await _userRepository.GetQueryAsync(criteriaForEmail, noTracking: true, cancellationToken: cancellationToken);

    if (userWithSameEmail.Count > 0)
    {
      _logger.LogWarning("User already exists with email:{Email}", request.CreateUserDto.Email);
      return Result.Failure<GetUserDto>(ApplicationError.UserEmailAlreadyExistsError(request.CreateUserDto.Email));
    }

    var passwordHash = _bcryptService.Hash(request.CreateUserDto.Password);

    var user = await _userRepository.CreateAsync(new Domain.Entities.User(
                                                   request.CreateUserDto.UserName,
                                                   request.CreateUserDto.Email,
                                                   passwordHash,
                                                   request.CreateUserDto.BaseCurrency), cancellationToken);

    var confirmationToken = await _tokenService.GenerateTokenAsync(user.Email, TokenType.EmailConfirmation);

    if (!confirmationToken.IsSuccess)
    {
      _logger.LogError("Failed to generate email confirmation token for user with ID:{Id}. Error: {Error}", user.Id, confirmationToken.ApplicationError?.Message);
      return Result.Failure<GetUserDto>(confirmationToken.ApplicationError!);
    }

    user.SetEmailConfirmationToken(confirmationToken.Data!, DateTimeOffset.UtcNow.AddHours(1));

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("User created with ID:{Id}", user.Id);

    var defaultGroups = CreateDefaultTransactionGroups(user);

    await _transactionGroupRepository.BatchCreateTransactionGroupsAsync(defaultGroups, cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Default transaction groups created for user with ID:{Id}", user.Id);

    var emailConfirmationResult = await _smtpEmailSender.SendEmailConfirmationAsync(user, confirmationToken.Data!);

    if (!emailConfirmationResult.IsSuccess)
    {
      _logger.LogError("Failed to send email confirmation to user with ID:{Id}", user.Id);
      return Result.Failure<GetUserDto>(ApplicationError.EmailConfirmationError(user.Email));
    }

    _logger.LogInformation("Email confirmation sent to user with ID:{Id}", user.Id);

    return Result.Success(_mapper.Map<GetUserDto>(user));
  }

  private List<Domain.Entities.TransactionGroup> CreateDefaultTransactionGroups(Domain.Entities.User user)
  {
    var groups = new List<Domain.Entities.TransactionGroup>
    {
      new Domain.Entities.TransactionGroup("Groceries", "Payment for groceries", Icons.ICON_GROCERIES, user),
      new Domain.Entities.TransactionGroup("Entertainment", "Expense for entertainment", Icons.ICON_ENTERTAINMENT, user),
      new Domain.Entities.TransactionGroup("Car", "Car cost and fuel", Icons.ICON_CAR, user),
      new Domain.Entities.TransactionGroup("Transport", "Transport costs like ticket or pass", Icons.ICON_TRANSPORT, user),
      new Domain.Entities.TransactionGroup("Health", "Medical and health-related expenses", Icons.ICON_HEALTH, user),
      new Domain.Entities.TransactionGroup("Utilities", "Utility bills and services", Icons.ICON_UTILITIES, user),
      new Domain.Entities.TransactionGroup("Education", "Education and learning expenses", Icons.ICON_EDUCATION, user),
      new Domain.Entities.TransactionGroup("Travel", "Travel and vacation expenses", Icons.ICON_TRAVEL, user),
      new Domain.Entities.TransactionGroup("Dining", "Restaurant and dining out", Icons.ICON_DINING, user),
      new Domain.Entities.TransactionGroup("Gifts", "Gifts and donations", Icons.ICON_GIFTS, user),
      new Domain.Entities.TransactionGroup("Salary", "Income from salary or wages", Icons.ICON_SALARY, user),
      new Domain.Entities.TransactionGroup("Home", "Home maintenance and rent", Icons.ICON_HOME, user),
      new Domain.Entities.TransactionGroup("Personal Care", "Personal care and hygiene", Icons.ICON_PERSONAL_CARE, user),
      new Domain.Entities.TransactionGroup("Clothing", "Clothing and accessories", Icons.ICON_CLOTHING, user),
      new Domain.Entities.TransactionGroup("Phone & Internet", "Phone, internet, and communication", Icons.ICON_PHONE_INTERNET, user),
      new Domain.Entities.TransactionGroup("Taxes", "Tax payments and refunds", Icons.ICON_TAXES, user),
      new Domain.Entities.TransactionGroup("Miscellaneous", "Other uncategorized expenses", Icons.ICON_MISCELLANEOUS, user)
    };

    return groups;
  }
}
