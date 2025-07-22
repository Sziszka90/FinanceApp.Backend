using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Backend.Application.Validators;

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
