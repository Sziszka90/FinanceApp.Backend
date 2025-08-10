using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetTopTransactionGroups;

public record GetTopTransactionGroupsQuery(
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    int Top
) : IQuery<Result<List<TopTransactionGroupDto>>>;