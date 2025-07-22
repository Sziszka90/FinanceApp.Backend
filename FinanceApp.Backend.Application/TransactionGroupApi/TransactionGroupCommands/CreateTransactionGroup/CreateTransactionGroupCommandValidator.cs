using System.Security.Claims;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;

public class CreateTransactionGroupCommandValidator : AbstractValidator<CreateTransactionGroupCommand>
{
  private readonly ILogger<CreateTransactionGroupCommandValidator> _logger;
  private readonly IHttpContextAccessor _httpContextAccessor;

  public CreateTransactionGroupCommandValidator(
    ILogger<CreateTransactionGroupCommandValidator> logger,
    IValidator<CreateTransactionGroupDto> createTransactionGroupDto,
    IHttpContextAccessor httpContextAccessor)
  {
    _logger = logger;
    _httpContextAccessor = httpContextAccessor;

    RuleFor(x => x.CreateTransactionGroupDto)
      .SetValidator(createTransactionGroupDto);

    RuleFor(x => x.CreateTransactionGroupDto)
      .Must(ValidateUserLoggedIn)
      .WithMessage(ApplicationError.USERNAME_NOT_LOGGED_IN_MESSAGE);
  }

  private bool ValidateUserLoggedIn(CreateTransactionGroupDto createTransactionDto)
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
