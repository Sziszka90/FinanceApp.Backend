using FinanceApp.Backend.Application.Models;
using FluentValidation;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ConfirmUserEmail;

public class ConfirmUserEmailCommandValidator : AbstractValidator<ConfirmUserEmailCommand>
{
  public ConfirmUserEmailCommandValidator()
  {

    RuleFor(x => x.Token)
      .NotEmpty()
      .WithMessage(ApplicationError.TOKEN_NOT_PROVIDED_MESSAGE);

    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage(ApplicationError.USER_ID_NOT_PROVIDED_MESSAGE);
  }
}
