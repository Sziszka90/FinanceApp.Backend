using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupCommands;

public record CreateExpenseGroupCommand(CreateExpenseTransactionGroupDto CreateExpenseTransactionGroupDto) : ICommand<Result<GetExpenseTransactionGroupDto>>;