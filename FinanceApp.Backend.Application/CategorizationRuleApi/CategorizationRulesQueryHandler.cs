using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.CategorizationRuleDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.CategorizationRuleApi;

public class CategorizationRulesQueryHandler : IQueryHandler<CategorizationRulesQuery, Result<List<CategorizationRuleDto>>>
{
  private readonly IRepository<CategorizationRule> _rules;
  private readonly ITransactionGroupRepository _groups;
  private readonly IUserService _users;

  public CategorizationRulesQueryHandler(IRepository<CategorizationRule> rules, ITransactionGroupRepository groups, IUserService users)
  {
    _rules = rules;
    _groups = groups;
    _users = users;
  }

  public async Task<Result<List<CategorizationRuleDto>>> Handle(CategorizationRulesQuery request, CancellationToken cancellationToken)
  {
    var user = await _users.GetActiveUserAsync(cancellationToken);
    if (!user.IsSuccess)
    {
      return Result.Failure<List<CategorizationRuleDto>>(user.ApplicationError!);
    }

    var rules = await _rules.GetAllAsync(rule => rule.UserId == user.Data!.Id, cancellationToken: cancellationToken);
    var groups = await _groups.GetAllByUserIdAsync(user.Data!.Id, noTracking: true, cancellationToken);
    var groupNames = groups.ToDictionary(group => group.Id, group => group.Name);
    return Result.Success(rules
      .Where(rule => groupNames.ContainsKey(rule.TransactionGroupId))
      .Select(rule => CategorizationRuleMapping.ToDto(rule, groupNames[rule.TransactionGroupId]))
      .OrderBy(rule => rule.MerchantText)
      .ToList());
  }
}
