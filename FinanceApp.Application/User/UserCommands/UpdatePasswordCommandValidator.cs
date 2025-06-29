using FinanceApp.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Application.User.UserCommands;

public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
  public UpdatePasswordCommandValidator(IValidator<UpdatePasswordDto> updatePasswordDto)
  {
    RuleFor(x => x.UpdatePasswordDto)
      .SetValidator(updatePasswordDto);
  }
}
