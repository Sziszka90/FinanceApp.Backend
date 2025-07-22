using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

public class CreateTransactionGroupDtoValidator : AbstractValidator<CreateTransactionGroupDto>
{
  public CreateTransactionGroupDtoValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);
  }
}
