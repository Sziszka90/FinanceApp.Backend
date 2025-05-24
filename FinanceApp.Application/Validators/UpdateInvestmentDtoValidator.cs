using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class UpdateInvestmentDtoValidator : AbstractValidator<UpdateInvestmentDto>
{
  #region Constructors

  public UpdateInvestmentDtoValidator(IValidator<Money> moneyValidator)
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);
    RuleFor(x => x.Amount)
      .SetValidator(moneyValidator);
  }

  #endregion
}
