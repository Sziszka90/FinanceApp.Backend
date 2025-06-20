using FinanceApp.Application.Dtos.ExchangeRateDtos;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Enums;

namespace FinanceApp.Application.Abstraction.HttpClients;

public interface ILLMClient
{
  public Task<Result<ExchangeRateResponseDto>> GetExchangeDataAsync(CurrencyEnum targetCurrency);
}
