using FinanceApp.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Abstraction.Clients;

public interface ILLMClient
{
  /// <summary>
  /// Retrieves exchange rate data for a specified target currency.
  /// The response is expected to be in a specific JSON format.
  /// The method uses a language model to generate the exchange rate data.
  /// </summary>
  /// <param name="targetCurrency"></param>
  /// <returns>Result<ExchangeRateResponseDto></returns>
  public Task<Result<ExchangeRateResponseDto>> GetExchangeDataAsync(CurrencyEnum targetCurrency);
}
