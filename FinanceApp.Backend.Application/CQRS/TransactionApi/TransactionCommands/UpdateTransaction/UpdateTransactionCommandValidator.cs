using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FluentValidation;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UpdateTransaction;

public class UpdateTransactionCommandValidator : AbstractValidator<UpdateTransactionCommand>
{
  public UpdateTransactionCommandValidator(IValidator<UpdateTransactionDto> updateTransactionDtoValidator)
  {
    RuleFor(x => x.UpdateTransactionDto)
      .SetValidator(updateTransactionDtoValidator);
  }
}
