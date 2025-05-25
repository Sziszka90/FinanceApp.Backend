using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public record DeleteTransactionCommand(Guid Id) : ICommand<Result>;
