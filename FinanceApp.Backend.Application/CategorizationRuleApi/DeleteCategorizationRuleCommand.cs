using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.CategorizationRuleApi;

public record DeleteCategorizationRuleCommand(Guid Id) : ICommand<Result<bool>>;
