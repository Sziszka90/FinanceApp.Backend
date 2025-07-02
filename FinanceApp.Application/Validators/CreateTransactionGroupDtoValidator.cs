using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Domain.Entities;
using FluentValidation;

namespace FinanceApp.Application.Validators;

public class CreateTransactionGroupDtoValidator : AbstractValidator<CreateTransactionGroupDto>
{
  public CreateTransactionGroupDtoValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty();
    RuleFor(x => x.Description)
      .MaximumLength(200);
  }
}
