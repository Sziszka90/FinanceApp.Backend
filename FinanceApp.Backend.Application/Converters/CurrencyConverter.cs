using FinanceApp.Backend.Domain.Enums;

namespace FinanceApp.Backend.Application.Converters;

public static class CurrencyConverter
{
  public static decimal ConvertToUserCurrency(decimal amount, CurrencyEnum fromCurrency, CurrencyEnum toCurrency, List<FinanceApp.Backend.Domain.Entities.ExchangeRate> rates)
  {
    if (fromCurrency == toCurrency)
      return Math.Round(amount, 2);

    var rate = rates.FirstOrDefault(r => r.BaseCurrency == fromCurrency.ToString() && r.TargetCurrency == toCurrency.ToString());
    if (rate == null)
      throw new InvalidOperationException($"Exchange rate not found for {fromCurrency} to {toCurrency}");

    return Math.Round(amount * rate.Rate, 2);
  }
}


