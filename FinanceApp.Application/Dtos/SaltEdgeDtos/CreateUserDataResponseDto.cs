using System.Text.Json.Serialization;

namespace FinanceApp.Application.Dtos.SaltEdgeDtos;

public class CreateUserDataResponseDto
{
  [JsonPropertyName("data")]
  public GetDataDto Data { get; set; } = new GetDataDto();
}

public class GetDataDto
{
  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("identifier")]
  public string Identifier { get; set; } = string.Empty;

  [JsonPropertyName("secret")]
  public string Secret { get; set; } = string.Empty;

  [JsonPropertyName("created_at")]
  public string CreatedAt { get; set; } = string.Empty;
}
