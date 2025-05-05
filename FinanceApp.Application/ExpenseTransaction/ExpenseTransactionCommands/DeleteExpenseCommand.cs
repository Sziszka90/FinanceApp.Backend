using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;

public record DeleteExpenseCommand(Guid Id) : ICommand<Result>;