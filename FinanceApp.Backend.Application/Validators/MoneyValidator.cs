using FinanceApp.Backend.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Backend.Application.ExpenseTransaction.ExpenseTransactionCommands;

public class MoneyValidator : AbstractValidator<Money>
{
  public MoneyValidator()
  {
    RuleFor(x => x.Amount)
      .NotEmpty()
      .GreaterThan(0);
  }
}
