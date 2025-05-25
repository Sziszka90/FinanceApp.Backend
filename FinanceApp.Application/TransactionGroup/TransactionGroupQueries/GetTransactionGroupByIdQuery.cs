using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupQueries;

public record GetTransactionGroupByIdQuery(Guid Id) : IQuery<Result<GetTransactionGroupDto>>;
