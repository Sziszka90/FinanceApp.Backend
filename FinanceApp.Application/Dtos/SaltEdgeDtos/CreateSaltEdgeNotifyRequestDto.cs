using System.Text.Json.Serialization;

namespace FinanceApp.Application.Dtos.SaltEdgeDtos;

public class CreateSaltEdgeNotifyRequestDto
{
  public DataDto Data { get; set; } = default!;

  public class DataDto
  {
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; } = default!;

    [JsonPropertyName("connection_id")]
    public string ConnectionId { get; set; } = default!;
    public string Status { get; set; } = default!;

    [JsonPropertyName("attempt_id")]
    public string AttemptId { get; set; } = default!;

    [JsonPropertyName("customer_fields")]
    public CustomFieldsDto CustomFields { get; set; } = default!;
  }

  public class CustomFieldsDto
  {
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = default!;
  }
}
