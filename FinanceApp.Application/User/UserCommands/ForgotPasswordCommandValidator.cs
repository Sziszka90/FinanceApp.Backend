using FinanceApp.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Application.User.UserCommands;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
  public ForgotPasswordCommandValidator(IValidator<EmailDto> emailDto)
  {
    RuleFor(x => x.EmailDto)
      .SetValidator(emailDto);
  }
}
