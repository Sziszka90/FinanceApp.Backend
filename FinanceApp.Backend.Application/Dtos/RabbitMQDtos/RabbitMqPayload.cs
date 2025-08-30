using System.Text.Json.Serialization;

namespace FinanceApp.Backend.Application.Dtos.RabbitMQDtos;

public class RabbitMqPayload
{
  [JsonPropertyName("correlation_id")]
  public required string CorrelationId { get; set; }

  [JsonPropertyName("success")]
  public required bool Success { get; set; }

  [JsonPropertyName("error")]
  public string? Error { get; set; }

  [JsonPropertyName("user_id")]
  public required string UserId { get; set; }

  [JsonPropertyName("prompt")]
  public required string Prompt { get; set; }

  [JsonPropertyName("response")]
  public required string Response { get; set; }
}
