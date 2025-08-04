using FinanceApp.Backend.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
  public ForgotPasswordCommandValidator(IValidator<EmailDto> emailDto)
  {
    RuleFor(x => x.EmailDto)
      .NotNull()
      .SetValidator(emailDto);
  }
}
