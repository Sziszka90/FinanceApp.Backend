using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Transaction.TransactionQueries;

public record GetAllTransactionQuery(TransactionFilter? TransactionFilter) : IQuery<Result<List<GetTransactionDto>>>;
