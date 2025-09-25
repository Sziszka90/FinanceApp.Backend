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
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName);

    // assert
    Assert.Contains("[Transaction]", result);
    Assert.Contains("WHERE UserId = @userId", result);
    Assert.Contains("TransactionGroupId IS NOT NULL", result);
    Assert.Contains("TransactionDate >= @startDate", result);
    Assert.Contains("TransactionDate <= @endDate", result);
    Assert.DoesNotContain("GROUP BY", result);
    Assert.DoesNotContain("ORDER BY", result);
    Assert.DoesNotContain("LIMIT", result);
    Assert.DoesNotContain("OFFSET", result);
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_Sqlite_ShouldGenerateCorrectSyntax()
  {
    // arrange
    var providerName = "Microsoft.EntityFrameworkCore.Sqlite";

    // act
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName);

    // assert
    Assert.Contains("\"Transaction\"", result);
    Assert.Contains("WHERE UserId = @userId", result);
    Assert.Contains("TransactionGroupId IS NOT NULL", result);
    Assert.Contains("TransactionDate >= @startDate", result);
    Assert.Contains("TransactionDate <= @endDate", result);
    Assert.DoesNotContain("GROUP BY", result);
    Assert.DoesNotContain("ORDER BY", result);
    Assert.DoesNotContain("LIMIT", result);
    Assert.DoesNotContain("OFFSET", result);
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_ShouldIncludeAllRequiredColumns()
  {
    // arrange
    var providerName = "Microsoft.EntityFrameworkCore.SqlServer";

    // act
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName);

    // assert
    Assert.Contains("[Transaction]", result);
    Assert.Contains("WHERE UserId = @userId", result);
    Assert.Contains("TransactionGroupId IS NOT NULL", result);
    Assert.Contains("TransactionDate >= @startDate", result);
    Assert.Contains("TransactionDate <= @endDate", result);
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_ShouldIncludeAllParameters()
  {
    // arrange
    var providerName = "Microsoft.EntityFrameworkCore.SqlServer";

    // act
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName);

    // assert
    Assert.Contains("@userId", result);
    Assert.Contains("@startDate", result);
    Assert.Contains("@endDate", result);
  }

  [Theory]
  [InlineData("Microsoft.EntityFrameworkCore.SqlServer")]
  [InlineData("Microsoft.EntityFrameworkCore.Sqlite")]
  public void BuildTransactionGroupAggregateQuery_ShouldReturnNonEmptyString(string providerName)
  {
    // act
    var result = _sqlQueryBuilder.BuildGetTransactionsByTopTransactionGroupsQuery(providerName);

    // assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Trim());
  }
}
