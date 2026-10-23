using FinanceApp.Backend.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
  public UpdateUserRequestValidator()
  {
    RuleFor(x => x.BaseCurrency)
      .IsInEnum();
  }
}
