using FinanceApp.Backend.Application.Models;
using FluentValidation;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.DeleteTransaction;

public class DeleteTransactionCommandValidator : AbstractValidator<DeleteTransactionCommand>
{
  public DeleteTransactionCommandValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage(ApplicationError.TRANSACTION_ID_REQUIRED_MESSAGE);
  }
}
