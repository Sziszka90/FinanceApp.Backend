using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionQueries;

public record GetExpenseByIdQuery(Guid Id) : IQuery<Result<GetExpenseTransactionDto>>;