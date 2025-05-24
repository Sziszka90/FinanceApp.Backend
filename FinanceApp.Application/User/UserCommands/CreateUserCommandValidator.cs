using FinanceApp.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Application.User.UserCommands;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
  public CreateUserCommandValidator(IValidator<CreateUserDto> createUserDto)
  {
    RuleFor(x => x.CreateUserDto)
      .SetValidator(createUserDto);
  }
}
