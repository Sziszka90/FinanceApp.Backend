using System.Text.Json.Serialization;

public class GetAccountsResponseDTO
{
    [JsonPropertyName("data")]
    public List<AccountDTO> Data { get; set; } = [];

    [JsonPropertyName("meta")]
    public MetaDTO Meta { get; set; } = new();
}

public class AccountDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("connection_id")]
    public string ConnectionId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("nature")]
    public string Nature { get; set; } = null!; // e.g., "iban", "card", "wallet"

    [JsonPropertyName("iban")]
    public string? Iban { get; set; }

    [JsonPropertyName("swift_code")]
    public string? SwiftCode { get; set; }

    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; } = null!;

    [JsonPropertyName("balance")]
    public decimal? Balance { get; set; }

    [JsonPropertyName("available_amount")]
    public decimal? AvailableAmount { get; set; }

    [JsonPropertyName("extra")]
    public Dictionary<string, object>? Extra { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}

public class MetaDTO
{
    [JsonPropertyName("next_id")]
    public string? NextId { get; set; }
}
