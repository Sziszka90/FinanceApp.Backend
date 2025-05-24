using FinanceApp.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Abstraction.HttpClients;

public interface IExchangeRateHttpClient
{
  public Task<Result<ExchangeRateResponseDto?>> GetDataAsync(string fromCurrency, string toCurrency);
}
