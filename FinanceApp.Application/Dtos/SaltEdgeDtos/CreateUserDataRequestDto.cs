using System.Text.Json.Serialization;

namespace FinanceApp.Application.Dtos.SaltEdgeDtos;

public class CreateUserDataRequestDto
{
  [JsonPropertyName("data")]
  public CreateDataDto Data { get; set; } = new CreateDataDto();
}

public class CreateDataDto
{
  [JsonPropertyName("identifier")]
  public string Identifier { get; set; } = string.Empty;
}

