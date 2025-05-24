using FinanceApp.Application.Dtos;
using FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
  #region Constructors

  public UpdateExpenseCommandValidator(IValidator<UpdateExpenseTransactionDto> updateExpenseTransactionDtoValidator)
  {
    RuleFor(x => x.UpdateExpenseTransactionDto)
      .SetValidator(updateExpenseTransactionDtoValidator);
  }

  #endregion
}
