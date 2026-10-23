using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

public class UpdateTransactionDtoValidator : AbstractValidator<UpdateTransactionDto>
{
  public UpdateTransactionDtoValidator(IValidator<Money> moneyValidator)
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);
    RuleFor(x => x.Value)
      .NotNull()
      .SetValidator(moneyValidator);
  }
}
