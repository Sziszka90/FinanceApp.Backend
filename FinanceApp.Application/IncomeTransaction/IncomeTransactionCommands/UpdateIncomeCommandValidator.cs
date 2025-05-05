using FinanceApp.Application.Dtos;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class UpdateIncomeCommandValidator : AbstractValidator<UpdateIncomeCommand>
{
  #region Constructors

  public UpdateIncomeCommandValidator(IValidator<UpdateIncomeTransactionDto> updateIncomeTransactionDtoValidator)
  {
    RuleFor(x => x.UpdateIncomeTransactionDto)
      .SetValidator(updateIncomeTransactionDtoValidator);
  }

  #endregion
}