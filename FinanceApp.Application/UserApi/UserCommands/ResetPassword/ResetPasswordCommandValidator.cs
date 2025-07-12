using FinanceApp.Application.Models;
using FluentValidation;

namespace FinanceApp.Application.UserApi.UserCommands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
  public ResetPasswordCommandValidator()
  {
    RuleFor(x => x.Token)
      .NotEmpty()
      .WithMessage(ApplicationError.TOKEN_NOT_PROVIDED_MESSAGE);
  }
}
