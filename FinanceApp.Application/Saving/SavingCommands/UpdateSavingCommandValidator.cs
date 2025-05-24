using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Application.Saving.SavingCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class UpdateSavingCommandValidator : AbstractValidator<UpdateSavingCommand>
{
  #region Constructors

  public UpdateSavingCommandValidator(IValidator<UpdateSavingDto> updateSavingDto)
  {
    RuleFor(x => x.UpdateSavingDto)
      .SetValidator(updateSavingDto);
  }

  #endregion
}
