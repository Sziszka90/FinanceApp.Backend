using System.Text.Json.Serialization;

namespace FinanceApp.Application.Dtos.SaltEdgeDtos;

public class CreateConnectionRequestDto
{
  public CreateConnectionDataRequestDto Data { get; set; } = default!;

  public class CreateConnectionDataRequestDto
  {
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; } = default!;
    public ConsentDto Consent { get; set; } = default!;
    public AttemptDto Attempt { get; set; } = default!;
  }

  public class ConsentDto
  {
    public List<string> Scopes { get; set; } = ["holder_info", "accounts", "transactions"];
  }

  public class AttemptDto
  {
    [JsonPropertyName("return_to")]
    public string ReturnTo { get; set; } = "www.financeapp.fun";
  }
}
