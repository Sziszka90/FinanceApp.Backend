using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class CreateInvestmentDtoValidator : AbstractValidator<CreateInvestmentDto>
{
  #region Constructors

  public CreateInvestmentDtoValidator(IValidator<Money> moneyValidator)
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