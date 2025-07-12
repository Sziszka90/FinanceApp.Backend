using System.Security.Claims;
using FinanceApp.Application.Abstraction.Repositories;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
  private readonly ILogger<CreateTransactionCommandValidator> _logger;
  private readonly IRepository<TransactionGroup> _transactionGroupRepository;
  private readonly IUserRepository _userRepository;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CreateTransactionCommandValidator(
    ILogger<CreateTransactionCommandValidator> logger,
    IValidator<CreateTransactionDto> createTransactionDtoValidator,
    IRepository<TransactionGroup> transactionGroupRepository,
    IUserRepository userRepository,
    IHttpContextAccessor httpContextAccessor)
  {
    _logger = logger;
    _transactionGroupRepository = transactionGroupRepository;
    _userRepository = userRepository;
    _httpContextAccessor = httpContextAccessor;

    RuleFor(x => x.CreateTransactionDto)
      .SetValidator(createTransactionDtoValidator);

    RuleFor(x => x.CreateTransactionDto)
      .Must(ValidateUserLoggedIn)
      .WithMessage(ApplicationError.USERNAME_NOT_LOGGED_IN_MESSAGE);
  }

  private bool ValidateUserLoggedIn(CreateTransactionDto createTransactionDto)
  {
    var httpContext = _httpContextAccessor.HttpContext;

    var userEmail = httpContext!.User.FindFirst(ClaimTypes.NameIdentifier)
                                      ?.Value;

    if (userEmail is null)
    {
      _logger.LogError("User is not logged in");
      return false;
    }
    return true;
  }
}
