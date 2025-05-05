using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Investment.InvestmentCommands;

public record DeleteInvestmentCommand(Guid Id) : ICommand<Result>;