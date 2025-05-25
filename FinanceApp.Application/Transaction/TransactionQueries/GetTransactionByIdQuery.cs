using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Transaction.TransactionQueries;

public record GetTransactionByIdQuery(Guid Id) : IQuery<Result<GetTransactionDto>>;
