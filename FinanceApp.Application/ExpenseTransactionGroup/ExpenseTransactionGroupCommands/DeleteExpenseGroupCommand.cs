using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;

public record DeleteExpenseGroupCommand(Guid Id) : ICommand<Result>;