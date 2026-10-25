using System.Text.Json.Serialization;

namespace FinanceApp.Backend.Application.Dtos.RabbitMQDtos;

public class CategorizationRuleJobDto
{
  [JsonPropertyName("merchant_text")]
  public required string MerchantText { get; set; }

  [JsonPropertyName("transaction_group_name")]
  public required string TransactionGroupName { get; set; }
}
