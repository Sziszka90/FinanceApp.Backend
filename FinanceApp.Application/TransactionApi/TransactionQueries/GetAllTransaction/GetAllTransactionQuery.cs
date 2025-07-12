using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionApi.TransactionQueries.GetAllTransaction;

public record GetAllTransactionQuery(TransactionFilter? TransactionFilter) : IQuery<Result<List<GetTransactionDto>>>;
