using FinanceApp.Backend.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.UserApi.UserCommands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
  public CreateUserCommandValidator(IValidator<CreateUserDto> createUserDto)
  {
    RuleFor(x => x.CreateUserDto)
      .SetValidator(createUserDto);
  }
}
