using FinanceApp.Backend.Application.Dtos.UserDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

public class EmailDtoValidator : AbstractValidator<EmailDto>
{
  public EmailDtoValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .WithMessage("Email cannot be empty.")
      .EmailAddress()
      .WithMessage("A valid email address is required.");
  }
}
