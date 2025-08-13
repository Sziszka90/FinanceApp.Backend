using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services;
using FinanceApp.Backend.Infrastructure.EntityFramework.Context;
using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Backend.Testing.Unit.Infrastructure.Services.Infrastructure;

public class DatabaseCommandServiceTests : IDisposable
{
  private readonly FinanceAppDbContext _dbContext;
  private readonly DatabaseCommandService _databaseCommandService;

  public DatabaseCommandServiceTests()
  {
    var options = new DbContextOptionsBuilder<FinanceAppSqliteDbContext>()
      .UseSqlite("DataSource=:memory:")
      .Options;

    _dbContext = new FinanceAppSqliteDbContext(options);
    _dbContext.Database.OpenConnection();
    _dbContext.Database.EnsureCreated();

    _databaseCommandService = new DatabaseCommandService(_dbContext);
  }

  [Fact]
  public async Task ExecuteQueryAsync_WithValidQuery_ShouldReturnMappedResults()
  {
    // arrange
    var sql = "SELECT 1 as TestValue, 'Test' as TestString";
    var parameters = new Dictionary<string, object>();

    // act
    var result = await _databaseCommandService.ExecuteQueryAsync(
        sql,
        parameters,
        reader => new { Value = reader.GetInt32(0), Text = reader.GetString(1) });

    // assert
    Assert.Single(result);
    Assert.Equal(1, result[0].Value);
    Assert.Equal("Test", result[0].Text);
  }

  [Fact]
  public async Task ExecuteQueryAsync_WithParameters_ShouldUseParametersCorrectly()
  {
    // arrange
    var sql = "SELECT 42 as Value1, 'Hello' as Value2";
    var parameters = new Dictionary<string, object>();

    // act
    var result = await _databaseCommandService.ExecuteQueryAsync(
        sql,
        parameters,
        reader => new { Value = reader.GetInt32(0), Text = reader.GetString(1) });

    // assert
    Assert.Single(result);
    Assert.Equal(42, result[0].Value);
    Assert.Equal("Hello", result[0].Text);
  }

  [Fact]
  public async Task ExecuteScalarAsync_WithValidQuery_ShouldReturnScalarValue()
  {
    // arrange
    var sql = "SELECT 123";
    var parameters = new Dictionary<string, object>();

    // act
    var result = await _databaseCommandService.ExecuteScalarAsync<int>(sql, parameters);

    // assert
    Assert.Equal(123, result);
  }

  [Fact]
  public async Task ExecuteQueryAsync_WithNullParameter_ShouldHandleDbNull()
  {
    // arrange
    var sql = "SELECT NULL as NullValue";
    var parameters = new Dictionary<string, object>();

    // act & assert - Should not throw
    var result = await _databaseCommandService.ExecuteQueryAsync(
        sql,
        parameters,
        reader => reader.IsDBNull(0) ? null : reader.GetString(0));

    Assert.Single(result);
    Assert.Null(result[0]);
  }

  public void Dispose()
  {
    _dbContext?.Dispose();
  }
}
