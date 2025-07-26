using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.DeleteTransaction;

public record DeleteTransactionCommand(Guid Id, CancellationToken CancellationToken) : ICommand<Result>;
