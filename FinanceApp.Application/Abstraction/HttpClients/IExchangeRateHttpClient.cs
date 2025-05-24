using FinanceApp.Application.Dtos.ExchangeRateDtos;

namespace FinanceApp.Application.Abstraction.HttpClients;

public interface IExchangeRateHttpClient
{
  #region Methods

  public Task<ExchangeRateResponseDto?> GetDataAsync(string fromCurrency, string toCurrency);

  #endregion
}
