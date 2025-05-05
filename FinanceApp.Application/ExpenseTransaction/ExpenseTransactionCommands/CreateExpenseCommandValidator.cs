using FinanceApp.Application.Dtos;
using FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
  #region Constructors

  public CreateExpenseCommandValidator(IValidator<CreateExpenseTransactionDto> createExpenseTransactionDtoValidator)
  {
    RuleFor(x => x.CreateExpenseTransactionDto)
      .SetValidator(createExpenseTransactionDtoValidator);
  }

  #endregion
}