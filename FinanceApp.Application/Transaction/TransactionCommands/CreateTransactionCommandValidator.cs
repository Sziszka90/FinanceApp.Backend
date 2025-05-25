using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Transaction.TransactionCommands;
using FluentValidation;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
  public CreateTransactionCommandValidator(IValidator<CreateTransactionDto> createTransactionDtoValidator)
  {
    RuleFor(x => x.CreateTransactionDto)
      .SetValidator(createTransactionDtoValidator);
  }
}
