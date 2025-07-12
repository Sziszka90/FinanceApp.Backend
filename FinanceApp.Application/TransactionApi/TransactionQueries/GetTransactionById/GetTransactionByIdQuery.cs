using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionApi.TransactionQueries.GetTransactionById;

public record GetTransactionByIdQuery(Guid Id, CancellationToken CancellationToken) : IQuery<Result<GetTransactionDto>>;
