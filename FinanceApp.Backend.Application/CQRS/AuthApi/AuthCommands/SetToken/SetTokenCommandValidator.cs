using FluentValidation;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.SetToken;

public class SetTokenCommandValidator : AbstractValidator<SetTokenCommand>
{
  public SetTokenCommandValidator()
  {
    RuleFor(x => x.Token)
      .NotEmpty()
      .WithMessage("Token is required.");

    RuleFor(x => x.RefreshToken)
      .NotEmpty()
      .WithMessage("Refresh token is required.");
  }
}
