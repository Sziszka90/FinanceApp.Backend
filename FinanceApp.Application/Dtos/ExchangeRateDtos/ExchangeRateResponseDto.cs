namespace FinanceApp.Application.Dtos.ExchangeRateDtos;

public class ExchangeRateResponseDto
{
  public string? Base { get; set; }
  public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
}

