using System.Text.Json.Serialization;

namespace FinanceApp.Application.Dtos.RabbitMQDtos;

public class RabbitMQResponseDto
{
  [JsonPropertyName("correlation_id")]
  public required string CorrelationId { get; set; }

  [JsonPropertyName("success")]
  public required bool Success { get; set; }

  [JsonPropertyName("user_id")]
  public required string UserId { get; set; }

  [JsonPropertyName("prompt")]
  public required string Prompt { get; set; }

  [JsonPropertyName("response")]
  public required string Response { get; set; }
}
