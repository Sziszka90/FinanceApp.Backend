using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Backend.Application.Dtos.CategorizationRuleDtos;

public class SaveCategorizationRuleDto
{
  [Required]
  [StringLength(200, MinimumLength = 2)]
  public string MerchantText { get; set; } = string.Empty;

  [Required]
  public Guid TransactionGroupId { get; set; }
}
