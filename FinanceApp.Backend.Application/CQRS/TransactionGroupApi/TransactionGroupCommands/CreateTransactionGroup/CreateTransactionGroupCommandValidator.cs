using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;

public class CreateTransactionGroupCommandValidator : AbstractValidator<CreateTransactionGroupCommand>
{
  private readonly ILogger<CreateTransactionGroupCommandValidator> _logger;
  private readonly IUserService _userService;

  public CreateTransactionGroupCommandValidator(
    ILogger<CreateTransactionGroupCommandValidator> logger,
    IValidator<CreateTransactionGroupDto> createTransactionGroupDto,
    IUserService userService)
  {
    _logger = logger;
    _userService = userService;

    RuleFor(x => x.CreateTransactionGroupDto)
      .SetValidator(createTransactionGroupDto);

    RuleFor(x => x.CreateTransactionGroupDto)
      .MustAsync(ValidateUserLoggedIn)
      .WithMessage(ApplicationError.USERNAME_NOT_LOGGED_IN_MESSAGE);
  }

  private async Task<bool> ValidateUserLoggedIn(CreateTransactionGroupDto dto, CancellationToken cancellationToken)
  {
    var userEmail = await _userService.GetActiveUserAsync(cancellationToken);

    if (userEmail is null)
    {
      _logger.LogError("User is not logged in");
      return false;
    }
    return true;
  }
}
