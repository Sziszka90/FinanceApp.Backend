using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetAllTransaction;

public record GetAllTransactionQuery(CancellationToken CancellationToken, TransactionFilter? TransactionFilter) : IQuery<Result<List<GetTransactionDto>>>;
