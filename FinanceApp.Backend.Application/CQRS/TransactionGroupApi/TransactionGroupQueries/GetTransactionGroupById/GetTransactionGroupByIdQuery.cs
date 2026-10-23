using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetTransactionGroupById;

public record GetTransactionGroupByIdQuery(Guid Id, CancellationToken CancellationToken) : IQuery<Result<GetTransactionGroupDto>>;
