using System.Text.Json.Serialization;

namespace FinanceApp.Backend.Application.Dtos.RabbitMQDtos;

public class PdfImportJobDto
{
  [JsonPropertyName("correlation_id")]
  public required string CorrelationId { get; set; }
  [JsonPropertyName("user_id")]
  public required string UserId { get; set; }
  [JsonPropertyName("filename")]
  public required string Filename { get; set; }
  [JsonPropertyName("pdf_base64")]
  public required string PdfBase64 { get; set; }
  [JsonPropertyName("prompt_id")]
  public string? PromptId { get; set; }
  [JsonPropertyName("prompt_name")]
  public string? PromptName { get; set; }
  [JsonPropertyName("prompt")]
  public string? Prompt { get; set; }
}
