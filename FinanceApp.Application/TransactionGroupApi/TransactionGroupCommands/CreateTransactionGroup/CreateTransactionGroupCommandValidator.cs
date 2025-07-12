using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FluentValidation;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;

public class CreateTransactionGroupCommandValidator : AbstractValidator<CreateTransactionGroupCommand>
{
  public CreateTransactionGroupCommandValidator(IValidator<CreateTransactionGroupDto> createTransactionGroupDto)
  {
    RuleFor(x => x.CreateTransactionGroupDto)
      .SetValidator(createTransactionGroupDto);
  }
}
