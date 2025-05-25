using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
  public CreateTransactionDtoValidator(IValidator<Money> moneyValidator)
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);
    RuleFor(x => x.Value)
      .SetValidator(moneyValidator);
  }
}
