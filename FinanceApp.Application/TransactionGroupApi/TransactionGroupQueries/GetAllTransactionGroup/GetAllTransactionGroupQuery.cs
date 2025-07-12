
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupQueries.GetAllTransactionGroup;

public record GetAllTransactionGroupQuery(CancellationToken CancellationToken) : IQuery<Result<List<GetTransactionGroupDto>>>;
