using FinanceApp.Backend.Application.Dtos.UserDtos;
using FinanceApp.Backend.Application.Models;
using FluentValidation;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ResendConfirmationEmail;

public class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
  public ResendConfirmationEmailCommandValidator(IValidator<EmailDto> emailDtoValidator)
  {
    RuleFor(x => x.EmailDto)
      .SetValidator(emailDtoValidator);
  }
}
