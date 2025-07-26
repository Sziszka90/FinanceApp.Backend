using AutoMapper;
using FinanceApp.Backend.Application.Abstraction.Clients;
using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Services;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandHandler : ICommandHandler<ResendConfirmationEmailCommand, Result<ResendEmailConfirmationResponse>>
{
  private readonly ILogger<ResendConfirmationEmailCommandHandler> _logger;
  private readonly IMapper _mapper;
  private readonly IUnitOfWork _unitOfWork;
  private readonly IUserRepository _userRepository;
  private readonly ISmtpEmailSender _smtpEmailSender;
  private readonly ITokenService _tokenService;

  public ResendConfirmationEmailCommandHandler(
    ILogger<ResendConfirmationEmailCommandHandler> logger,
    IMapper mapper,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    ISmtpEmailSender smtpEmailSender,
    ITokenService tokenService)
  {
    _logger = logger;
    _mapper = mapper;
    _userRepository = userRepository;
    _unitOfWork = unitOfWork;
    _smtpEmailSender = smtpEmailSender;
    _tokenService = tokenService;
  }

  /// <inheritdoc />
  public async Task<Result<ResendEmailConfirmationResponse>> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
  {
    var user = await _userRepository.GetUserByEmailAsync(request.EmailDto.Email, noTracking: false, cancellationToken: cancellationToken);

    if (user is null)
    {
      _logger.LogWarning("User not found with email: {Email}", request.EmailDto.Email);
      return Result.Failure<ResendEmailConfirmationResponse>(ApplicationError.UserNotFoundError(request.EmailDto.Email));
    }

    if (user.IsEmailConfirmed)
    {
      _logger.LogInformation("Email already confirmed for user: {UserId}", user.Id);
      return Result.Success(new ResendEmailConfirmationResponse
      {
        IsSuccess = true,
        Message = "Email already confirmed."
      });
    }

    var confirmationToken = await _tokenService.GenerateTokenAsync(user.Email, TokenType.EmailConfirmation);

    if (!confirmationToken.IsSuccess)
    {
      _logger.LogError("Failed to generate email confirmation token for user: {Email}", user.Email);
      return Result.Failure<ResendEmailConfirmationResponse>(confirmationToken.ApplicationError!);
    }

    user.EmailConfirmationToken = confirmationToken.Data;
    user.EmailConfirmationTokenExpiration = DateTimeOffset.UtcNow.AddHours(24);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    var emailConfirmationResult = await _smtpEmailSender.SendEmailConfirmationAsync(user, confirmationToken.Data!);

    if (!emailConfirmationResult.IsSuccess)
    {
      _logger.LogError("Failed to send email confirmation to {Email}", user.Email);
      return Result.Failure<ResendEmailConfirmationResponse>(emailConfirmationResult.ApplicationError!);
    }

    _logger.LogInformation("Email confirmation resent to {Email}", user.Email);

    return Result.Success(new ResendEmailConfirmationResponse
    {
      IsSuccess = true,
      Message = "Email confirmation resent successfully."
    });
  }
}
