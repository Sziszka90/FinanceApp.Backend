using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Application.Saving.SavingCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class CreateUserCommandValidator : AbstractValidator<CreateSavingCommand>
{
  #region Constructors

  public CreateUserCommandValidator(IValidator<CreateSavingDto> createSavingDto)
  {
    RuleFor(x => x.CreateSavingDto)
      .SetValidator(createSavingDto);
  }

  #endregion
}
