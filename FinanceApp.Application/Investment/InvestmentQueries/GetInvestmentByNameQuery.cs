using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Investment.InvestmentQueries;

public record GetInvestmentByNameQuery(Guid Id) : IQuery<Result<GetInvestmentDto>>;