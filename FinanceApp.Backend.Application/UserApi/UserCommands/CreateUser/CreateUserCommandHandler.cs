using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.QueryCriteria;
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
  private readonly IJwtService _jwtService;

  public CreateUserCommandHandler(
    ILogger<CreateUserCommandHandler> logger,
    IMapper mapper,
    IRepository<Domain.Entities.User> userRepository,
    ITransactionGroupRepository transactionGroupRepository,
    IUnitOfWork unitOfWork,
    ISmtpEmailSender smtpEmailSender,
    IBcryptService bcryptService,
    IJwtService jwtService)
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
    _transactionGroupRepository = transactionGroupRepository;
    _unitOfWork = unitOfWork;
    _smtpEmailSender = smtpEmailSender;
    _bcryptService = bcryptService;
    _jwtService = jwtService;
  }

  /// <inheritdoc />
  public async Task<Result<GetUserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
  {
    var criteriaForUserName = UserQueryCriteria.FindUserName(request.CreateUserDto);

    var usersWithSameName = await _userRepository.GetQueryAsync(criteriaForUserName, noTracking: true, cancellationToken: cancellationToken);

    if (usersWithSameName.Count > 0)
    {
      _logger.LogError("User already exists with name:{Name}", request.CreateUserDto.UserName);
      return Result.Failure<GetUserDto>(ApplicationError.UserNameAlreadyExistsError(request.CreateUserDto.UserName));
    }

    var criteriaForEmail = UserQueryCriteria.FindUserEmail(request.CreateUserDto);

    var userWithSameEmail = await _userRepository.GetQueryAsync(criteriaForEmail, noTracking: true, cancellationToken: cancellationToken);

    if (userWithSameEmail.Count > 0)
    {
      _logger.LogError("User already exists with email:{Email}", request.CreateUserDto.Email);
      return Result.Failure<GetUserDto>(ApplicationError.UserEmailAlreadyExistsError(request.CreateUserDto.Email));
    }

    var passwordHash = _bcryptService.Hash(request.CreateUserDto.Password);

    var user = await _userRepository.CreateAsync(new Domain.Entities.User(
                                                   request.CreateUserDto.UserName,
                                                   request.CreateUserDto.Email,
                                                   passwordHash,
                                                   request.CreateUserDto.BaseCurrency), cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);
    _logger.LogDebug("User created with ID:{Id}", user.Id);

    var defaultGroups = CreateDefaultTransactionGroups(user);

    await _transactionGroupRepository.BatchCreateTransactionGroupsAsync(defaultGroups, cancellationToken);

    var confirmationToken = _jwtService.GenerateToken(user.Email);
    user.EmailConfirmationToken = confirmationToken;
    user.EmailConfirmationTokenExpiration = DateTimeOffset.UtcNow.AddHours(1);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    _logger.LogDebug("User created with ID:{Id}", user.Id);

    var emailConfirmationResult = await _smtpEmailSender.SendEmailConfirmationAsync(user, confirmationToken);

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
      new Domain.Entities.TransactionGroup("Groceries", "Payment for groceries", Icons.IconGroceries, user),
      new Domain.Entities.TransactionGroup("Entertainment", "Expense for entertainment", Icons.IconEntertainment, user),
      new Domain.Entities.TransactionGroup("Car", "Car cost and fuel", Icons.IconCar, user),
      new Domain.Entities.TransactionGroup("Transport", "Transport costs like ticket or pass", Icons.IconTransport, user),
      new Domain.Entities.TransactionGroup("Health", "Medical and health-related expenses", Icons.IconHealth, user),
      new Domain.Entities.TransactionGroup("Utilities", "Utility bills and services", Icons.IconUtilities, user),
      new Domain.Entities.TransactionGroup("Education", "Education and learning expenses", Icons.IconEducation, user),
      new Domain.Entities.TransactionGroup("Travel", "Travel and vacation expenses", Icons.IconTravel, user),
      new Domain.Entities.TransactionGroup("Dining", "Restaurant and dining out", Icons.IconDining, user),
      new Domain.Entities.TransactionGroup("Gifts", "Gifts and donations", Icons.IconGifts, user),
      new Domain.Entities.TransactionGroup("Salary", "Income from salary or wages", Icons.IconSalary, user),
      new Domain.Entities.TransactionGroup("Home", "Home maintenance and rent", Icons.IconHome, user),
      new Domain.Entities.TransactionGroup("Personal Care", "Personal care and hygiene", Icons.IconPersonalCare, user),
      new Domain.Entities.TransactionGroup("Clothing", "Clothing and accessories", Icons.IconClothing, user),
      new Domain.Entities.TransactionGroup("Phone & Internet", "Phone, internet, and communication", Icons.IconPhoneInternet, user),
      new Domain.Entities.TransactionGroup("Taxes", "Tax payments and refunds", Icons.IconTaxes, user),
      new Domain.Entities.TransactionGroup("Miscellaneous", "Other uncategorized expenses", Icons.IconMiscellaneous, user)
    };

    return groups;
  }
}
