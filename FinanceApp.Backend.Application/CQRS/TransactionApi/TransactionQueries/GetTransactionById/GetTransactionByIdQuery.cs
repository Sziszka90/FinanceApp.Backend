using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionById;

public record GetTransactionByIdQuery(Guid Id, CancellationToken CancellationToken) : IQuery<Result<GetTransactionDto>>;
