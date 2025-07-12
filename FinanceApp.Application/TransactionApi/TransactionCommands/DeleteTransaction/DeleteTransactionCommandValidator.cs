using FinanceApp.Application.Models;
using FluentValidation;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.DeleteTransaction;

public class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
  public DeleteTransactionCommandValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage(ApplicationError.TRANSACTION_ID_REQUIRED_MESSAGE);
  }
}
