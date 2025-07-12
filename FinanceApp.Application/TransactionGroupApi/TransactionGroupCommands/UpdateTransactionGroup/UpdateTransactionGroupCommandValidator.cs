using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FluentValidation;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.UpdateTransactionGroup;

public class UpdateTransactionGroupCommandValidator : AbstractValidator<UpdateTransactionGroupCommand>
{
  public UpdateTransactionGroupCommandValidator(IValidator<UpdateTransactionGroupDto> updateTransactionGroupDto)
  {
    RuleFor(x => x.UpdateTransactionGroupDto)
      .SetValidator(updateTransactionGroupDto);
  }
}
