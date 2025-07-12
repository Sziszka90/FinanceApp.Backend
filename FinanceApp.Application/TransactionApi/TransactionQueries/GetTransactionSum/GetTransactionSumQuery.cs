using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.TransactionApi.TransactionQueries.GetTransactionSum;

public record GetTransactionSumQuery(CancellationToken CancellationToken) : IQuery<Result<Money>>;
