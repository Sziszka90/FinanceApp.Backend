using FinanceApp.Application.Dtos;
using FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class CreateIncomeGroupCommandValidator : AbstractValidator<CreateIncomeGroupCommand>
{
  public CreateIncomeGroupCommandValidator(IValidator<CreateIncomeTransactionGroupDto> createIncomeTransactionGroupDto)
  {
    RuleFor(x => x.CreateIncomeTransactionGroupDto)
      .SetValidator(createIncomeTransactionGroupDto);
  }
}
