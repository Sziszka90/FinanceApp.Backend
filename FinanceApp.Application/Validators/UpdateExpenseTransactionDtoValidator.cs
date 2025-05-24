using FinanceApp.Application.Dtos;
using FinanceApp.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class UpdateExpenseTransactionDtoValidator : AbstractValidator<UpdateExpenseTransactionDto>
{
  public UpdateExpenseTransactionDtoValidator(IValidator<Money> moneyValidator)
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);
    RuleFor(x => x.Value)
      .SetValidator(moneyValidator);
  }
}
