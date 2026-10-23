using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services;

namespace FinanceApp.Backend.Testing.Unit.Infrastructure.Services.Infrastructure;

public class SqlQueryBuilderTests
{
  private readonly SqlQueryBuilder _sqlQueryBuilder;

  public SqlQueryBuilderTests()
  {
    _sqlQueryBuilder = new SqlQueryBuilder();
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_SqlServer_ShouldGenerateCorrectSyntax()
  {
    // arrange
    var providerName = "Microsoft.EntityFrameworkCore.SqlServer";

    // act
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName, 10);

    // assert
    Assert.Contains("[Transaction]", result);
    Assert.Contains("[TransactionGroup]", result);
    Assert.Contains("TOP (@top)", result);
    Assert.Contains("INNER JOIN", result);
    Assert.Contains("GROUP BY tg.Id", result);
    Assert.Contains("ORDER BY SUM(t2.ValueInBaseCurrency) DESC", result);
    Assert.Contains("WHERE t.UserId = @userId", result);
    Assert.Contains("WHERE t2.UserId = @userId", result);
    Assert.Contains("TransactionDate BETWEEN @startDate AND @endDate", result);
    Assert.DoesNotContain("LIMIT", result);
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_Sqlite_ShouldGenerateCorrectSyntax()
  {
    // arrange
    var providerName = "Microsoft.EntityFrameworkCore.Sqlite";

    // act
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName, 10);

    // assert
    Assert.Contains("\"Transaction\"", result);
    Assert.Contains("\"TransactionGroup\"", result);
    Assert.Contains("LIMIT @top", result);
    Assert.Contains("INNER JOIN", result);
    Assert.Contains("GROUP BY tg.Id", result);
    Assert.Contains("ORDER BY SUM(t2.ValueInBaseCurrency) DESC", result);
    Assert.Contains("WHERE t.UserId = @userId", result);
    Assert.Contains("WHERE t2.UserId = @userId", result);
    Assert.Contains("TransactionDate BETWEEN @startDate AND @endDate", result);
    Assert.DoesNotContain("TOP", result);
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_ShouldIncludeAllRequiredColumns()
  {
    // arrange
    var providerName = "Microsoft.EntityFrameworkCore.SqlServer";

    // act
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName, 10);

    // assert
    Assert.Contains("SELECT t.*", result);
    Assert.Contains("SELECT TOP (@top) tg.Id", result);
    Assert.Contains("FROM [Transaction] t", result);
    Assert.Contains("INNER JOIN", result);
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_ShouldIncludeAllParameters()
  {
    // arrange
    var providerName = "Microsoft.EntityFrameworkCore.SqlServer";

    // act
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName, 10);

    // assert
    Assert.Contains("@userId", result);
    Assert.Contains("@startDate", result);
    Assert.Contains("@endDate", result);
    Assert.Contains("@top", result);
  }

  [Theory]
  [InlineData("Microsoft.EntityFrameworkCore.SqlServer")]
  [InlineData("Microsoft.EntityFrameworkCore.Sqlite")]
  public void BuildTransactionGroupAggregateQuery_ShouldReturnNonEmptyString(string providerName)
  {
    // act
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName, 10);

    // assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Trim());
  }
}
