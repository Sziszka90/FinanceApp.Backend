using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Transaction.TransactionQueries;

public record GetTransactionByNameQuery(Guid Id) : IQuery<Result<GetTransactionDto>>;
