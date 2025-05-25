using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.TransactionGroup.TransactionGroupCommands;
using FluentValidation;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public class CreateTransactionGroupCommandValidator : AbstractValidator<CreateTransactionGroupCommand>
{
  public CreateTransactionGroupCommandValidator(IValidator<CreateTransactionGroupDto> createTransactionGroupDto)
  {
    RuleFor(x => x.CreateTransactionGroupDto)
      .SetValidator(createTransactionGroupDto);
  }
}
