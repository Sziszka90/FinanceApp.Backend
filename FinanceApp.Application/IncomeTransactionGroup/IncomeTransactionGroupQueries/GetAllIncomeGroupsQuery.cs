using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupQueries;

public record GetAllIncomeGroupsQuery : IQuery<Result<List<GetIncomeTransactionGroupDto>>>;