using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
  private readonly ILogger<CreateTransactionCommandValidator> _logger;
  private readonly IUserService _userService;

  public CreateTransactionCommandValidator(
    ILogger<CreateTransactionCommandValidator> logger,
    IValidator<CreateTransactionDto> createTransactionDtoValidator,
    IUserService userService)
  {
    _logger = logger;
    _userService = userService;

    RuleFor(x => x.CreateTransactionDto)
      .SetValidator(createTransactionDtoValidator);

    RuleFor(x => x.CreateTransactionDto)
      .MustAsync(ValidateUserLoggedIn)
      .WithMessage(ApplicationError.USERNAME_NOT_LOGGED_IN_MESSAGE);
  }

  private async Task<bool> ValidateUserLoggedIn(CreateTransactionDto dto, CancellationToken cancellationToken)
  {
    var user = await _userService.GetActiveUserAsync(cancellationToken);

    if (!user.IsSuccess)
    {
      _logger.LogError("Failed to retrieve active user: {Error}", user.ApplicationError?.Message);
      return false;
    }
    return true;
  }
}
