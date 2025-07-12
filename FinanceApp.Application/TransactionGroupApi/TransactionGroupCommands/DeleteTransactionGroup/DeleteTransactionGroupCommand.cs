using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.DeleteTransactionGroup;

public record DeleteTransactionGroupCommand(Guid Id) : ICommand<Result>;
