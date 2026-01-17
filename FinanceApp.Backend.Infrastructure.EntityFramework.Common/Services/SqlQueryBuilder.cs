using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services;

public class SqlQueryBuilder : ISqlQueryBuilder
{
  public string BuildGetTransactionsByTopTransactionGroupsQuery(string providerName, int? top)
  {
    var isSqlServer = providerName == "Microsoft.EntityFrameworkCore.SqlServer";

    var transactionTable = isSqlServer ? "[Transaction]" : "\"Transaction\"";
    var transactionGroupTable = isSqlServer ? "[TransactionGroup]" : "\"TransactionGroup\"";
    var topClause = top.HasValue && isSqlServer ? "TOP (@top)" : "";
    var limitClause = top.HasValue && !isSqlServer ? "LIMIT @top" : "";
    var orderByClause = top.HasValue ? "ORDER BY SUM(t2.ValueInBaseCurrency) DESC" : "";

    return $@"SELECT t.*
      FROM {transactionTable} t
      INNER JOIN (
          SELECT {topClause} tg.Id
          FROM {transactionTable} t2
          INNER JOIN {transactionGroupTable} tg ON t2.TransactionGroupId = tg.Id
          WHERE t2.UserId = @userId
            AND t2.TransactionDate BETWEEN @startDate AND @endDate
          GROUP BY tg.Id
          {orderByClause}
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
