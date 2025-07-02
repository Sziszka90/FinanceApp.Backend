using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class UpdateTransactionGroupDtoValidator : AbstractValidator<UpdateTransactionGroupDto>
{
  public UpdateTransactionGroupDtoValidator(IValidator<Money> moneyValidator)
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);
  }
}
