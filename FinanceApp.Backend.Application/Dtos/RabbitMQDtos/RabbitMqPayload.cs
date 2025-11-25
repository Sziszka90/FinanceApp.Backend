namespace FinanceApp.Backend.Application.Dtos.RabbitMQDtos;

public class RabbitMqPayload
{
  public required string CorrelationId { get; set; }
  public required bool Success { get; set; }
  public string? Error { get; set; }
  public required string UserId { get; set; }
  public required string Prompt { get; set; }
}
