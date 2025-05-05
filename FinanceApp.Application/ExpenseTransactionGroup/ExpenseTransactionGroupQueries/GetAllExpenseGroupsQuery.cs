using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.ExpenseTransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.ExpenseTransactionGroup.ExpenseTransactionGroupQueries;

public record GetAllExpenseGroupsQuery : IQuery<Result<List<GetExpenseTransactionGroupDto>>>;