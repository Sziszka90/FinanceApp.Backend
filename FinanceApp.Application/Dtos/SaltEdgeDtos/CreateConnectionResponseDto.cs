using System.Text.Json.Serialization;

namespace FinanceApp.Application.Dtos.SaltEdgeDtos;

public class CreateConnectionResponseDto
{
  public CreateConnectionDataResponseDto Data { get; set; } = null!;
}

public class CreateConnectionDataResponseDto
{
  [JsonPropertyName("customer_id")]
  public string CustomerId { get; set; } = null!;
  [JsonPropertyName("connect_url")]
  public string ConnectUrl { get; set; } = null!;
  [JsonPropertyName("expires_at")]
  public string ExpiresAt { get; set; } = null!;
}
