using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public record DeleteIncomeCommand(Guid Id) : ICommand<Result>;
