using FluentValidation;

namespace FinanceApp.Backend.Application.AuthApi.AuthCommands.Refresh;

public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
  public RefreshCommandValidator()
  {
    RuleFor(x => x.RefreshToken)
      .NotEmpty()
      .WithMessage("Refresh token is required.");
  }
}
