using System.Text.Json.Serialization;

public class GetAccountsRequestDto
{
    [JsonPropertyName("data")]
    public List<AccountDto> Data { get; set; } = [];

    [JsonPropertyName("meta")]
    public MetaDto Meta { get; set; } = new();
}

public class AccountDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    [JsonPropertyName("connection_id")]
    public string ConnectionId { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("iban")]
    public string? Iban { get; set; }

    [JsonPropertyName("currency_code")]
    public string CurrencyCode { get; set; } = null!;

    [JsonPropertyName("nature")]
    public string Nature { get; set; } = null!;

    [JsonPropertyName("balance")]
    public decimal? Balance { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonPropertyName("extra")]
    public AccountExtraDto? Extra { get; set; }
}

public class AccountExtraDto
{
    [JsonPropertyName("holder")]
    public string? Holder { get; set; }
}

public class MetaDto
{
    [JsonPropertyName("next_id")]
    public string? NextId { get; set; }
}
