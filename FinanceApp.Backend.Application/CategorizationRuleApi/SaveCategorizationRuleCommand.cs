using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.CategorizationRuleDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.CategorizationRuleApi;

public record SaveCategorizationRuleCommand(SaveCategorizationRuleDto Rule, Guid? Id = null)
  : ICommand<Result<CategorizationRuleDto>>;
