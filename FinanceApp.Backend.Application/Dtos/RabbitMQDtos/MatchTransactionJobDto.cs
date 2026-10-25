using System.Text.Json.Serialization;

namespace FinanceApp.Backend.Application.Dtos.RabbitMQDtos;

public class MatchTransactionJobDto
{
  [JsonPropertyName("transaction_names")]
  public required List<string> TransactionNames { get; set; }

  [JsonPropertyName("transaction_group_names")]
  public required List<string> TransactionGroupNames { get; set; }

  [JsonPropertyName("categorization_rules")]
  public List<CategorizationRuleJobDto> CategorizationRules { get; set; } = [];

  [JsonPropertyName("user_id")]
  public required string UserId { get; set; }

  [JsonPropertyName("correlation_id")]
  public required string CorrelationId { get; set; }
}
