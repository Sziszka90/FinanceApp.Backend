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
    // Arrange
    var providerName = "Microsoft.EntityFrameworkCore.SqlServer";

    // Act
    var result = _sqlQueryBuilder.BuildTransactionGroupAggregateQuery(providerName);

    // Assert
    Assert.Contains("[Transaction]", result);
    Assert.Contains("[TransactionGroup]", result);
    Assert.Contains("OFFSET 0 ROWS FETCH NEXT @topCount ROWS ONLY", result);
    Assert.DoesNotContain("LIMIT", result);
    Assert.Contains("WHERE t.UserId = @userId", result);
    Assert.Contains("GROUP BY tg.Id, t.Currency", result);
    Assert.Contains("ORDER BY SUM(t.Amount) DESC", result);
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_Sqlite_ShouldGenerateCorrectSyntax()
  {
    // Arrange
    var providerName = "Microsoft.EntityFrameworkCore.Sqlite";

    // Act
    var result = _sqlQueryBuilder.BuildTransactionGroupAggregateQuery(providerName);

    // Assert
    Assert.Contains("\"Transaction\"", result);
    Assert.Contains("\"TransactionGroup\"", result);
    Assert.Contains("LIMIT @topCount", result);
    Assert.DoesNotContain("OFFSET", result);
    Assert.DoesNotContain("FETCH", result);
    Assert.Contains("WHERE t.UserId = @userId", result);
    Assert.Contains("GROUP BY tg.Id, t.Currency", result);
    Assert.Contains("ORDER BY SUM(t.Amount) DESC", result);
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_ShouldIncludeAllRequiredColumns()
  {
    // Arrange
    var providerName = "Microsoft.EntityFrameworkCore.SqlServer";

    // Act
    var result = _sqlQueryBuilder.BuildTransactionGroupAggregateQuery(providerName);

    // Assert
    Assert.Contains("tg.Id as TransactionGroupId", result);
    Assert.Contains("tg.Name", result);
    Assert.Contains("tg.Description", result);
    Assert.Contains("tg.GroupIcon", result);
    Assert.Contains("t.Currency", result);
    Assert.Contains("SUM(t.Amount) as TotalAmount", result);
    Assert.Contains("COUNT(*) as TransactionCount", result);
  }

  [Fact]
  public void BuildTransactionGroupAggregateQuery_ShouldIncludeAllParameters()
  {
    // Arrange
    var providerName = "Microsoft.EntityFrameworkCore.SqlServer";

    // Act
    var result = _sqlQueryBuilder.BuildTransactionGroupAggregateQuery(providerName);

    // Assert
    Assert.Contains("@userId", result);
    Assert.Contains("@startDate", result);
    Assert.Contains("@endDate", result);
    Assert.Contains("@topCount", result);
  }

  [Theory]
  [InlineData("Microsoft.EntityFrameworkCore.SqlServer")]
  [InlineData("Microsoft.EntityFrameworkCore.Sqlite")]
  public void BuildTransactionGroupAggregateQuery_ShouldReturnNonEmptyString(string providerName)
  {
    // Act
    var result = _sqlQueryBuilder.BuildTransactionGroupAggregateQuery(providerName);

    // Assert
    Assert.NotNull(result);
    Assert.NotEmpty(result.Trim());
  }
}
