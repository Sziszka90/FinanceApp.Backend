using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupCommands;

public record DeleteIncomeGroupCommand(Guid Id) : ICommand<Result>;
