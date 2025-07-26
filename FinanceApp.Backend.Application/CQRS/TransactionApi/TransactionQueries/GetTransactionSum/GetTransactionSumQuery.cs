using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionSum;

public record GetTransactionSumQuery(CancellationToken CancellationToken) : IQuery<Result<Money>>;
