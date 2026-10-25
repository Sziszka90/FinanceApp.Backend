namespace FinanceApp.Backend.Application.Dtos.CategorizationRuleDtos;

public class CategorizationRuleDto
{
  public Guid Id { get; set; }
  public string MerchantText { get; set; } = string.Empty;
  public Guid TransactionGroupId { get; set; }
  public string TransactionGroupName { get; set; } = string.Empty;
}
