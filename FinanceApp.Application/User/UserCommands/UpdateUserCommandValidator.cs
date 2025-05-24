using FinanceApp.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Application.User.UserCommands;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
  public UpdateUserCommandValidator(IValidator<UpdateUserDto> updateUserDto)
  {
    RuleFor(x => x.UpdateUserDto)
      .SetValidator(updateUserDto);
  }
}
