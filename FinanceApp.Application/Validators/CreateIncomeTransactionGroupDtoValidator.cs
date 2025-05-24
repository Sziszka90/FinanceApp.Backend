using FinanceApp.Application.Dtos;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class CreateIncomeTransactionGroupDtoValidator : AbstractValidator<CreateIncomeTransactionGroupDto>
{
  public CreateIncomeTransactionGroupDtoValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);
  }
}
