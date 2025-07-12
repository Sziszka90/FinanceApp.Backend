using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupQueries.GetTransactionGroupById;

public record GetTransactionGroupByIdQuery(Guid Id, CancellationToken CancellationToken) : IQuery<Result<GetTransactionGroupDto>>;
