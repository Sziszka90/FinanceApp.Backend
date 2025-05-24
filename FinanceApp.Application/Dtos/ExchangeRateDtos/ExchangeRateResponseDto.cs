namespace FinanceApp.Application.Dtos.ExchangeRateDtos;

public class ExchangeRateResponseDto
{
  #region Properties

  public bool Success { get; set; }

  public double TimeStamp { get; set; }

  public required string Base { get; set; }
  public DateTimeOffset Date { get; set; }

  public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();

  #endregion
}

