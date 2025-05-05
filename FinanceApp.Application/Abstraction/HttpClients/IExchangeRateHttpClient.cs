namespace FinanceApp.Application.Abstraction.HttpClients;

public interface IExchangeRateHttpClient
{
  #region Methods

  public Task<Dictionary<string, decimal>?> GetDataAsync(string fromCurrency, string toCurrency);

  #endregion
}