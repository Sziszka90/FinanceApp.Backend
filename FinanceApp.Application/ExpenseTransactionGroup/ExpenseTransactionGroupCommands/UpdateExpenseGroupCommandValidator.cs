using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class UpdateExpenseGroupCommandValidator : AbstractValidator<UpdateExpenseGroupCommand>
{
  #region Constructors

  public UpdateExpenseGroupCommandValidator(IValidator<UpdateExpenseTransactionGroupDto> updateExpenseTransactionGroupDto)
  {
    RuleFor(x => x.UpdateExpenseTransactionGroupDto)
      .SetValidator(updateExpenseTransactionGroupDto);
  }

  #endregion
}
