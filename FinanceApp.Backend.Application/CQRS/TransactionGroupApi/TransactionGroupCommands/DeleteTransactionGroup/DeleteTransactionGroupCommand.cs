using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.DeleteTransactionGroup;

public record DeleteTransactionGroupCommand(Guid Id, CancellationToken CancellationToken) : ICommand<Result>;
