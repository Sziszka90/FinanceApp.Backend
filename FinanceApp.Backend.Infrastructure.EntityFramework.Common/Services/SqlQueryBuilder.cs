using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services;

public class SqlQueryBuilder : ISqlQueryBuilder
{
  public string BuildGetTransactionsByTopTransactionGroupsQuery(string providerName)
  {
    var isSqlServer = providerName == "Microsoft.EntityFrameworkCore.SqlServer";

    var transactionTable = isSqlServer ? "[Transaction]" : "\"Transaction\"";

    return $@"
      SELECT *
      FROM {transactionTable}
      WHERE UserId = @userId
        AND TransactionGroupId IS NOT NULL
        AND TransactionDate >= @startDate
        AND TransactionDate <= @endDate";
  }

  public string BuildGetExchangeRatesByDateRangeQuery(string providerName)
  {
    var isSqlServer = providerName == "Microsoft.EntityFrameworkCore.SqlServer";

    var exchangeRateTable = isSqlServer ? "[ExchangeRate]" : "\"ExchangeRate\"";

    return $@"
      SELECT *
      FROM {exchangeRateTable}
      WHERE ValidFrom <= @date
        AND (ValidTo IS NULL OR ValidTo >= @date)
      ORDER BY Actual DESC, ValidFrom DESC";
  }
}
