
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetAllTransactionGroup;

public record GetAllTransactionGroupQuery(CancellationToken CancellationToken) : IQuery<Result<List<GetTransactionGroupDto>>>;
