namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;

public interface ISqlQueryBuilder
{
  /// <summary>
  /// Build the GetTransactionsByTopTransactionGroups SQL query.
  /// </summary>
  /// <param name="providerName"></param>
  /// <returns></returns>
  string BuildGetTransactionsByTopTransactionGroupsQuery(string providerName);

  /// <summary>
  /// Build the GetExchangeRatesByDateRange SQL query.
  /// </summary>
  /// <param name="providerName"></param>
  /// <returns></returns>
  string BuildGetExchangeRatesByDateRangeQuery(string providerName);
}
