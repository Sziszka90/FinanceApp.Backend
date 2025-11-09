using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services;

public class SqlQueryBuilder : ISqlQueryBuilder
{
  public string BuildGetTransactionsByTopTransactionGroupsQuery(string providerName)
  {
    var isSqlServer = providerName == "Microsoft.EntityFrameworkCore.SqlServer";

    var transactionTable = isSqlServer ? "[Transaction]" : "\"Transaction\"";
    var transactionGroupTable = isSqlServer ? "[TransactionGroup]" : "\"TransactionGroup\"";
    var topClause = isSqlServer ? "TOP (@top)" : "";
    var limitClause = isSqlServer ? "" : "LIMIT @top";

    return $@"SELECT t.*
      FROM {transactionTable} t
      INNER JOIN (
          SELECT {topClause} tg.Id
          FROM {transactionTable} t2
          INNER JOIN {transactionGroupTable} tg ON t2.TransactionGroupId = tg.Id
          WHERE t2.UserId = @userId
            AND t2.TransactionDate BETWEEN @startDate AND @endDate
          GROUP BY tg.Id
          ORDER BY SUM(t2.ValueInBaseCurrency) DESC
          {limitClause}
      ) AS topGroups ON t.TransactionGroupId = topGroups.Id
      WHERE t.UserId = @userId
        AND t.TransactionDate BETWEEN @startDate AND @endDate";
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
