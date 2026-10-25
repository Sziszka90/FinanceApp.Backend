using FinanceApp.Backend.Application.Dtos.CategorizationRuleDtos;
using FinanceApp.Backend.Domain.Entities;

namespace FinanceApp.Backend.Application.CategorizationRuleApi;

internal static class CategorizationRuleMapping
{
  public static CategorizationRuleDto ToDto(CategorizationRule rule, string transactionGroupName) => new()
  {
    Id = rule.Id,
    MerchantText = rule.MerchantText,
    TransactionGroupId = rule.TransactionGroupId,
    TransactionGroupName = transactionGroupName
  };
}
