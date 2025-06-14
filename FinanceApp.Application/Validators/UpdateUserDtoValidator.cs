using FinanceApp.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
  public UpdateUserDtoValidator()
  {
    RuleFor(x => x.BaseCurrency)
      .IsInEnum();
  }
}
