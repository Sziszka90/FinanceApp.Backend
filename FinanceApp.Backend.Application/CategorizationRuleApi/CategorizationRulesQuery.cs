using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.CategorizationRuleDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.CategorizationRuleApi;

public record CategorizationRulesQuery : IQuery<Result<List<CategorizationRuleDto>>>;
