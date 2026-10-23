using FluentValidation;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.ValidateToken;

public class ValidateTokenCommandValidator : AbstractValidator<ValidateTokenCommand>
{
  public ValidateTokenCommandValidator()
  {
    RuleFor(x => x.validateTokenRequest.Token)
      .NotEmpty()
      .WithMessage("Token is required.");

    RuleFor(x => x.validateTokenRequest.TokenType)
      .NotNull()
      .WithMessage("Token type is required.");
  }
}
