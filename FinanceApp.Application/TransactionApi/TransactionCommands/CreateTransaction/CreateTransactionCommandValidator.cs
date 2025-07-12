using FinanceApp.Application.Dtos.TransactionDtos;
using FluentValidation;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.CreateTransaction;

public class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
  public CreateTransactionCommandValidator(IValidator<CreateTransactionDto> createTransactionDtoValidator)
  {
    RuleFor(x => x.CreateTransactionDto)
      .SetValidator(createTransactionDtoValidator);
  }
}
