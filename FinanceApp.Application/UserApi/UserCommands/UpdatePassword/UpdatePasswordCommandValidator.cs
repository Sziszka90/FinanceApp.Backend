using FinanceApp.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Application.UserApi.UserCommands.UpdatePassword;

public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
  public UpdatePasswordCommandValidator(IValidator<UpdatePasswordRequest> updatePasswordDto)
  {
    RuleFor(x => x.UpdatePasswordDto)
      .SetValidator(updatePasswordDto);
  }
}
