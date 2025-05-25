using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.TransactionGroup.TransactionGroupCommands;
using FluentValidation;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public class UpdateTransactionGroupCommandValidator : AbstractValidator<UpdateTransactionGroupCommand>
{
  public UpdateTransactionGroupCommandValidator(IValidator<UpdateTransactionGroupDto> updateTransactionGroupDto)
  {
    RuleFor(x => x.UpdateTransactionGroupDto)
      .SetValidator(updateTransactionGroupDto);
  }
}
