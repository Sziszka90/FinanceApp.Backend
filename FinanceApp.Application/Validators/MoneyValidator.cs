using FinanceApp.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;

public class MoneyValidator : AbstractValidator<Money>
{
  #region Constructors

  public MoneyValidator()
  {
    RuleFor(x => x.Amount)
      .NotEmpty()
      .GreaterThan(0);
  }

  #endregion
}