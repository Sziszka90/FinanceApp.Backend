using FinanceApp.Application.Dtos.UserDtos;
using FinanceApp.Application.Models;
using FluentValidation;

namespace FinanceApp.Application.UserApi.UserCommands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
  public ResendConfirmationEmailCommandValidator(IValidator<EmailDto> emailDtoValidator)
  {
    RuleFor(x => x.EmailDto)
      .SetValidator(emailDtoValidator);
  }
}
