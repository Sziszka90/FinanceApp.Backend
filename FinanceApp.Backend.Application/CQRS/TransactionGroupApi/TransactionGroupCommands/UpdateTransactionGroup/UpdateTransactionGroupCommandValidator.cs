using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.UpdateTransactionGroup;

public class UpdateTransactionGroupCommandValidator : AbstractValidator<UpdateTransactionGroupCommand>
{
  public UpdateTransactionGroupCommandValidator(IValidator<UpdateTransactionGroupDto> updateTransactionGroupDto)
  {
    RuleFor(x => x.UpdateTransactionGroupDto)
      .SetValidator(updateTransactionGroupDto);
  }
}
