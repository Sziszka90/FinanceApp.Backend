using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Investment.InvestmentQueries;

public record GetAllInvestmentsQuery : IQuery<Result<List<GetInvestmentDto>>>;
