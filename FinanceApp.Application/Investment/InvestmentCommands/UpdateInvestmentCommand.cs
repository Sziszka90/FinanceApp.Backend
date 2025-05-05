using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.InvestmentDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Investment.InvestmentCommands;

public record UpdateInvestmentCommand(UpdateInvestmentDto UpdateInvestmentDto) : ICommand<Result<GetInvestmentDto>>;