using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupQueries;

public record GetTransactionGroupImageQuery(Guid Id) : IQuery<Result<Icon>>;
