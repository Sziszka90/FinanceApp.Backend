namespace FinanceApp.Application.Dtos.ExchangeRateDtos;

public class ExchangeRateResponseDto
{
  public string Disclaimer { get; set; } = string.Empty;
  public string License { get; set; } = string.Empty;
  public long Timestamp { get; set; }
  public string? Base { get; set; }
  public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
}
