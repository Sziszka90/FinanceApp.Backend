using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.ExpenseTransaction.ExpenseTransactionQueries;

public record GetExpenseSumQuery : IQuery<Result<Money>>;
