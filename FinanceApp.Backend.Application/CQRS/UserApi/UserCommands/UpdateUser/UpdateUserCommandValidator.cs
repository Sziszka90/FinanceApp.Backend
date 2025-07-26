using FinanceApp.Backend.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
  public UpdateUserCommandValidator(IValidator<UpdateUserRequest> updateUserDto)
  {
    RuleFor(x => x.UpdateUserDto)
      .SetValidator(updateUserDto);
  }
}
