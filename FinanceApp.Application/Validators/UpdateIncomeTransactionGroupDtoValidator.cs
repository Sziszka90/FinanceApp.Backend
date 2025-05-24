using FinanceApp.Application.Dtos;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class UpdateIncomeTransactionGroupDtoValidator : AbstractValidator<UpdateIncomeTransactionGroupDto>
{
  public UpdateIncomeTransactionGroupDtoValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);
  }
}
