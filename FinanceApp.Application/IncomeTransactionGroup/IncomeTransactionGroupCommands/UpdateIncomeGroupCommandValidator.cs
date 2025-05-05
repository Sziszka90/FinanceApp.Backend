using FinanceApp.Application.Dtos;
using FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class UpdateIncomeGroupCommandValidator : AbstractValidator<UpdateIncomeGroupCommand>
{
  #region Constructors

  public UpdateIncomeGroupCommandValidator(IValidator<UpdateIncomeTransactionGroupDto> updateIncomeTransactionGroupDto)
  {
    RuleFor(x => x.UpdateIncomeTransactionGroupDto)
      .SetValidator(updateIncomeTransactionGroupDto);
  }

  #endregion
}