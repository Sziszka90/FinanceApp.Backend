using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Application.Investment.InvestmentCommands;
using FluentValidation;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public class CreateInvestmentCommandValidator : AbstractValidator<CreateInvestmentCommand>
{
  #region Constructors

  public CreateInvestmentCommandValidator(IValidator<CreateInvestmentDto> createInvestmentDto)
  {
    RuleFor(x => x.CreateInvestmentDto)
      .SetValidator(createInvestmentDto);
  }

  #endregion
}
