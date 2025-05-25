using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupCommands;

public record DeleteTransactionGroupCommand(Guid Id) : ICommand<Result>;
