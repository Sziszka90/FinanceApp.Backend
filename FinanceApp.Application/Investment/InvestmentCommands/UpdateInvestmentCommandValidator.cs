using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Application.Investment.InvestmentCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class UpdateInvestmentCommandValidator : AbstractValidator<UpdateInvestmentCommand>
{
  #region Constructors

  public UpdateInvestmentCommandValidator(IValidator<UpdateInvestmentDto> updateInvestmentDto)
  {
    RuleFor(x => x.UpdateInvestmentDto)
      .SetValidator(updateInvestmentDto);
  }

  #endregion
}
