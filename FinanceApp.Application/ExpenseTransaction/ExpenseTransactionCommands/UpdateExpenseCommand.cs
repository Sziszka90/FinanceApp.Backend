using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionCommands;

public record UpdateExpenseCommand(UpdateExpenseTransactionDto UpdateExpenseTransactionDto) : ICommand<Result<GetExpenseTransactionDto>>;