using System.Text.Json.Serialization;

namespace FinanceApp.Backend.Application.Dtos.RabbitMQDtos;

public class PdfImportResultDto
{
  [JsonPropertyName("correlation_id")]
  public required string CorrelationId { get; set; }
  [JsonPropertyName("user_id")]
  public required string UserId { get; set; }
  [JsonPropertyName("success")]
  public bool Success { get; set; }
  [JsonPropertyName("error")]
  public string? Error { get; set; }
  [JsonPropertyName("data")]
  public PdfStatementDto? Data { get; set; }
}

public class PdfStatementDto
{
  [JsonPropertyName("statementCurrency")]
  public string? StatementCurrency { get; set; }
  [JsonPropertyName("openingBalance")]
  [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
  public decimal? OpeningBalance { get; set; }
  [JsonPropertyName("closingBalance")]
  [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
  public decimal? ClosingBalance { get; set; }
  [JsonPropertyName("totalDebits")]
  [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
  public decimal? TotalDebits { get; set; }
  [JsonPropertyName("totalCredits")]
  [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
  public decimal? TotalCredits { get; set; }
  [JsonPropertyName("transactions")]
  public List<PdfTransactionDto> Transactions { get; set; } = [];
}

public class PdfTransactionDto
{
  [JsonPropertyName("description")]
  public string? Description { get; set; }
  [JsonPropertyName("details")]
  public string? Details { get; set; }
  [JsonPropertyName("date")]
  public string? Date { get; set; }
  [JsonPropertyName("signedAmount")]
  public string? SignedAmount { get; set; }
  [JsonPropertyName("currency")]
  public string? Currency { get; set; }
  [JsonPropertyName("reference")]
  public string? Reference { get; set; }
  [JsonPropertyName("pageNumber")]
  public int? PageNumber { get; set; }
}
