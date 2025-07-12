using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.DeleteTransaction;

public record DeleteTransactionCommand(Guid Id, CancellationToken CancellationToken) : ICommand<Result>;
