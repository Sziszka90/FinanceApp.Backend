using FinanceApp.Application.Dtos.TransactionDtos;
using FluentValidation;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
  public UpdateTransactionCommandValidator(IValidator<UpdateTransactionDto> updateTransactionDtoValidator)
  {
    RuleFor(x => x.UpdateTransactionDto)
      .SetValidator(updateTransactionDtoValidator);
  }
}
