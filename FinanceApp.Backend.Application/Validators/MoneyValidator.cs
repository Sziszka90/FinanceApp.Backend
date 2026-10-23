using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FluentValidation;

namespace FinanceApp.Backend.Application.ExpenseTransaction.ExpenseTransactionCommands;

public class MoneyValidator : AbstractValidator<Money>
{
  public MoneyValidator()
  {
    RuleFor(x => x.Amount)
      .NotEmpty()
      .GreaterThan(0);
    RuleFor(x => x.Currency)
      .IsInEnum()
      .NotEqual(CurrencyEnum.XXX);
  }
}
