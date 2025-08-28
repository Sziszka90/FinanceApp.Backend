using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services;

public class SqlQueryBuilder : ISqlQueryBuilder
{
  public string BuildTransactionGroupAggregateQuery(string providerName)
  {
    var isSqlServer = providerName == "Microsoft.EntityFrameworkCore.SqlServer";
    var transactionTable = isSqlServer ? "[Transaction]" : "\"Transaction\"";
    var transactionGroupTable = isSqlServer ? "[TransactionGroup]" : "\"TransactionGroup\"";
    var limitClause = isSqlServer ? "OFFSET 0 ROWS FETCH NEXT @topCount ROWS ONLY" : "LIMIT @topCount";

    return $@"
      SELECT
        tg.Id as TransactionGroupId,
        tg.Name,
        tg.Description,
        tg.GroupIcon,
        t.Currency,
        SUM(t.Amount) as TotalAmount,
        COUNT(*) as TransactionCount
      FROM {transactionTable} t
      INNER JOIN {transactionGroupTable} tg ON t.TransactionGroupId = tg.Id
      WHERE t.UserId = @userId AND t.TransactionGroupId IS NOT NULL
        AND t.TransactionDate >= @startDate AND t.TransactionDate <= @endDate
      GROUP BY tg.Id, tg.Name, tg.Description, tg.GroupIcon, t.Currency
      ORDER BY SUM(t.Amount) DESC
      {limitClause}";
  }
}
