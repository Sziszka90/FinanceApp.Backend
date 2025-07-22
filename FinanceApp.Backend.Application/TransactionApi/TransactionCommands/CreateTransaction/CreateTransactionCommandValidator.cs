using System.Security.Claims;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
  private readonly ILogger<CreateTransactionCommandValidator> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CreateTransactionCommandValidator(
    ILogger<CreateTransactionCommandValidator> logger,
    IValidator<CreateTransactionDto> createTransactionDtoValidator,
    IHttpContextAccessor httpContextAccessor)
  {
    _logger = logger;
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
