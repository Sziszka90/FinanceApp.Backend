namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;

public interface ISqlQueryBuilder
{
  /// <summary>
  /// Builds a SQL query for aggregating transaction groups.
  /// </summary>
  /// <param name="providerName"></param>
  /// <returns></returns>
  string BuildTransactionGroupAggregateQuery(string providerName);
}
