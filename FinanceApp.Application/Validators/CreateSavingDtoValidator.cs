using FinanceApp.Application.Dtos.SavingDtos;
using FinanceApp.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class CreateSavingDtoValidator : AbstractValidator<CreateSavingDto>
{
  #region Constructors

  public CreateSavingDtoValidator(IValidator<Money> moneyValidator)
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
