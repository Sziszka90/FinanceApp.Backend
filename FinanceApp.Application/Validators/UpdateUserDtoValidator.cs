using FinanceApp.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
  #region Constructors

  public UpdateUserDtoValidator()
  {
    RuleFor(x => x.UserName)
      .NotEmpty();
    RuleFor(x => x.Password)
      .NotEmpty()
      .WithMessage("Password cannot be empty.")
      .MinimumLength(8)
      .WithMessage("Password must be at least 8 characters long.")
      .Matches("[A-Z]")
      .WithMessage("Password must contain at least one uppercase letter.")
      .Matches("[a-z]")
      .WithMessage("Password must contain at least one lowercase letter.")
      .Matches("[0-9]")
      .WithMessage("Password must contain at least one digit.")
      .Matches("[^a-zA-Z0-9]")
      .WithMessage("Password must contain at least one special character.");
    RuleFor(x => x.BaseCurrency)
      .IsInEnum();
  }

  #endregion
}