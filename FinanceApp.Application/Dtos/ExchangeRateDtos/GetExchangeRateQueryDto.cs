namespace FinanceApp.Application.Dtos.ExchangeRateDtos;

public class GetExchangeRateQueryDto
{
  #region Properties

  public string Source { get; set; }
  public List<string> Targets { get; set; }

  #endregion
}