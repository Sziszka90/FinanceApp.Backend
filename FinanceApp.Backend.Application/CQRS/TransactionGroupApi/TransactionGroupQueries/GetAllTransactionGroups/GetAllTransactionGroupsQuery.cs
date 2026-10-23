
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetAllTransactionGroups;

public record GetAllTransactionGroupsQuery(CancellationToken CancellationToken) : IQuery<Result<List<GetTransactionGroupDto>>>;
