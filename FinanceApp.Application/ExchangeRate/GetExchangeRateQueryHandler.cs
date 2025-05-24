using FinanceApp.Application.Abstraction.HttpClients;
using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionQueries;

public class GetExchangeRateQueryHandler : IQueryHandler<GetExchangeRateQuery, Result<Dictionary<string, decimal>?>>
{
  #region Members

  private readonly IExchangeRateHttpClient _exchangeRateHttpClient;

  #endregion

  #region Constructors

  public GetExchangeRateQueryHandler(IExchangeRateHttpClient exchangeRateHttpClient)
  {
    _exchangeRateHttpClient = exchangeRateHttpClient;
  }

  #endregion

  #region Methods

  public async Task<Result<Dictionary<string, decimal>?>> Handle(GetExchangeRateQuery request, CancellationToken cancellationToken)
  {
    //var result = await _exchangeRateHttpClient.GetDataAsync();
    //return Result.Success(result);
    return Result.Success<Dictionary<string, decimal>?>(new Dictionary<string, decimal>());
  }

  #endregion
}
