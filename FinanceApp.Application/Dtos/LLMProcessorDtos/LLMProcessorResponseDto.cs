using System.Text.Json.Serialization;

namespace FinanceApp.Application.LLMProcessorDtos;

public class LLMProcessorResponseDto
{
  [JsonPropertyName("status")]
  public required string Status { get; set; }
  [JsonPropertyName("correlation_id")]
  public required string CorrelationId { get; set; }
  [JsonPropertyName("message")]
  public required string Message { get; set; }
}

