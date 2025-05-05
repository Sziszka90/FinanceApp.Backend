using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class UpdateExpenseTransactionGroupDtoValidator : AbstractValidator<UpdateExpenseTransactionGroupDto>
{
  #region Constructors

  public UpdateExpenseTransactionGroupDtoValidator(IValidator<Money> moneyValidator)
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);

    When(x => x.Limit is not null, () =>
                                   {
                                     RuleFor(x => x.Limit)
                                       .SetValidator(moneyValidator!);
                                   });
  }

  #endregion
}