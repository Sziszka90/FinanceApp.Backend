using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class CreateExpenseGroupCommandValidator : AbstractValidator<CreateExpenseGroupCommand>
{
  public CreateExpenseGroupCommandValidator(IValidator<CreateExpenseTransactionGroupDto> createExpenseTransactionGroupDto)
  {
    RuleFor(x => x.CreateExpenseTransactionGroupDto)
      .SetValidator(createExpenseTransactionGroupDto);
  }
}
