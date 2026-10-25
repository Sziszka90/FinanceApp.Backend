using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.CategorizationRuleDtos;
using FinanceApp.Backend.Application.Hubs;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.CategorizationRuleApi;

public class SaveCategorizationRuleCommandHandler : ICommandHandler<SaveCategorizationRuleCommand, Result<CategorizationRuleDto>>
{
  private readonly IRepository<CategorizationRule> _rules;
  private readonly ITransactionGroupRepository _groups;
  private readonly ITransactionRepository _transactions;
  private readonly IUserService _users;
  private readonly IUnitOfWork _unitOfWork;
  private readonly ISignalRService _signalR;

  public SaveCategorizationRuleCommandHandler(IRepository<CategorizationRule> rules, ITransactionGroupRepository groups,
    ITransactionRepository transactions, IUserService users, IUnitOfWork unitOfWork, ISignalRService signalR)
  {
    _rules = rules;
    _groups = groups;
    _transactions = transactions;
    _users = users;
    _unitOfWork = unitOfWork;
    _signalR = signalR;
  }

  public async Task<Result<CategorizationRuleDto>> Handle(SaveCategorizationRuleCommand request, CancellationToken cancellationToken)
  {
    var user = await _users.GetActiveUserAsync(cancellationToken);
    if (!user.IsSuccess)
    {
      return Result.Failure<CategorizationRuleDto>(user.ApplicationError!);
    }

    var merchantText = request.Rule.MerchantText.Trim();
    if (merchantText.Length < 2 || merchantText.Length > 200 || request.Rule.TransactionGroupId == Guid.Empty)
    {
      return Result.Failure<CategorizationRuleDto>(ApplicationError.DefaultError("Enter a match text and select a category."));
    }

    var group = (await _groups.GetAllByUserIdAsync(user.Data!.Id, noTracking: true, cancellationToken))
      .FirstOrDefault(item => item.Id == request.Rule.TransactionGroupId);
    if (group is null)
    {
      return Result.Failure<CategorizationRuleDto>(ApplicationError.DefaultError("The selected category does not exist."));
    }

    var userRules = await _rules.GetAllAsync(rule => rule.UserId == user.Data.Id, cancellationToken: cancellationToken);
    var duplicate = userRules
      .Any(rule => rule.Id != request.Id && string.Equals(rule.MerchantText, merchantText, StringComparison.OrdinalIgnoreCase));
    if (duplicate)
    {
      return Result.Failure<CategorizationRuleDto>(ApplicationError.DefaultError("A rule for this match text already exists."));
    }

    CategorizationRule rule;
    if (request.Id is Guid id)
    {
      var existing = await _rules.GetSingleOrDefaultAsync(item => item.Id == id && item.UserId == user.Data.Id,
        cancellationToken: cancellationToken);
      if (existing is null)
      {
        return Result.Failure<CategorizationRuleDto>(ApplicationError.EntityNotFoundError(id.ToString(), nameof(CategorizationRule)));
      }
      rule = existing;
      rule.Update(merchantText, group.Id);
    }
    else
    {
      rule = new CategorizationRule
      {
        UserId = user.Data.Id,
        MerchantText = merchantText,
        TransactionGroupId = group.Id
      };
      await _rules.CreateAsync(rule, cancellationToken);
      userRules.Add(rule);
    }

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    var groups = await _groups.GetAllByUserIdAsync(user.Data.Id, noTracking: true, cancellationToken);
    var groupsById = groups.ToDictionary(item => item.Id);
    var rulesBySpecificity = userRules.OrderByDescending(item => item.MerchantText.Length).ToList();
    var transactions = await _transactions.GetAllByUserIdAsync(user.Data.Id, noTracking: false, cancellationToken);
    var changed = false;

    foreach (var transaction in transactions)
    {
      var matchingRule = rulesBySpecificity.FirstOrDefault(item =>
        transaction.Name.Contains(item.MerchantText, StringComparison.OrdinalIgnoreCase));
      if (matchingRule is null || !groupsById.ContainsKey(matchingRule.TransactionGroupId) ||
          transaction.TransactionGroupId == matchingRule.TransactionGroupId)
      {
        continue;
      }

      transaction.TransactionGroupId = matchingRule.TransactionGroupId;
      changed = true;
    }

    if (changed)
    {
      await _unitOfWork.SaveChangesAsync(cancellationToken);
      await _signalR.SendToClientGroupMethodAsync(user.Data.Email.ToString(),
        HubConstants.TRANSACTIONS_MATCHED_NOTIFICATION, HubConstants.REFRESH_TRANSACTIONS);
    }

    return Result.Success(CategorizationRuleMapping.ToDto(rule, group.Name));
  }
}
