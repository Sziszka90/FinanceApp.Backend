using FluentValidation;

namespace FinanceApp.Application.AuthApi.AuthCommands.ValidateToken;

public class ValidateTokenCommandValidator : AbstractValidator<ValidateTokenCommand>
{
  public ValidateTokenCommandValidator()
  {
    RuleFor(x => x.Token)
      .NotEmpty()
      .WithMessage("Token is required.");
  }
}
