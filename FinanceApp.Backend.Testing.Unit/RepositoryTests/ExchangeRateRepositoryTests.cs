using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;
using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.RepositoryTests;

public class ExchangeRateRepositoryTests : IDisposable
{
  private readonly FinanceAppSqliteDbContext _dbContext;
  private readonly Mock<IFilteredQueryProvider> _filteredQueryProviderMock;
  private readonly ExchangeRateRepository _repository;

  public ExchangeRateRepositoryTests()
  {
    var options = new DbContextOptionsBuilder<FinanceAppSqliteDbContext>()
      .UseSqlite("DataSource=:memory:")
      .Options;

    _dbContext = new FinanceAppSqliteDbContext(options);
    _dbContext.Database.OpenConnection();
    _dbContext.Database.EnsureCreated();

    _filteredQueryProviderMock = new Mock<IFilteredQueryProvider>();
    _filteredQueryProviderMock.Setup(x => x.Query<ExchangeRate>()).Returns(_dbContext.Set<ExchangeRate>().AsQueryable());

    var sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
    var databaseCommandServiceMock = new Mock<IDatabaseCommandService>();

    _repository = new ExchangeRateRepository(
      _dbContext,
      _filteredQueryProviderMock.Object,
      sqlQueryBuilderMock.Object,
      databaseCommandServiceMock.Object
    );
  }

  public void Dispose()
  {
    try
    {
      _dbContext?.Database?.CloseConnection();
    }
    catch (ObjectDisposedException)
    {
      // Context is already disposed, ignore
    }

    _dbContext?.Dispose();
  }

  public class GetExchangeRatesAsyncTests : ExchangeRateRepositoryTests
  {
    [Fact]
    public async Task GetExchangeRatesAsync_WithTracking_ShouldReturnExchangeRates()
    {
      // arrange
      var exchangeRates = new List<ExchangeRate>
      {
        new ExchangeRate("USD", "EUR", 0.85m)
        {
          Id = Guid.NewGuid(),
          Actual = true,
          ValidFrom = DateTimeOffset.UtcNow,
          ValidTo = null
        },
        new ExchangeRate("USD", "GBP", 0.73m)
        {
          Id = Guid.NewGuid(),
          Actual = true,
          ValidFrom = DateTimeOffset.UtcNow,
          ValidTo = null
        }
      };

      await _dbContext.Set<ExchangeRate>().AddRangeAsync(exchangeRates);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetExchangeRatesAsync(noTracking: false);

      // assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Contains(result, r => r.BaseCurrency == "USD" && r.TargetCurrency == "EUR" && r.Rate == 0.85m);
      Assert.Contains(result, r => r.BaseCurrency == "USD" && r.TargetCurrency == "GBP" && r.Rate == 0.73m);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithNoTracking_ShouldReturnExchangeRates()
    {
      // arrange
      var exchangeRate = new ExchangeRate("USD", "EUR", 0.85m)
      {
        Id = Guid.NewGuid(),
        Actual = true,
        ValidFrom = DateTimeOffset.UtcNow,
        ValidTo = null
      };

      await _dbContext.Set<ExchangeRate>().AddAsync(exchangeRate);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetExchangeRatesAsync(noTracking: true);

      // assert
      Assert.NotNull(result);
      Assert.Single(result);
      Assert.Equal("USD", result[0].BaseCurrency);
      Assert.Equal("EUR", result[0].TargetCurrency);
      Assert.Equal(0.85m, result[0].Rate);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
      // arrange
      // No setup needed - database is empty

      // act
      var result = await _repository.GetExchangeRatesAsync();

      // assert
      Assert.NotNull(result);
      Assert.Empty(result);
    }

    [Fact]
    public async Task GetExchangeRatesAsync_WhenExceptionThrown_ShouldThrowDatabaseException()
    {
      // arrange
      // Dispose the context to simulate an error
      _dbContext.Dispose();

      // act & assert
      var exception = await Assert.ThrowsAsync<DatabaseException>(() => _repository.GetExchangeRatesAsync());
      Assert.Equal("GET_ALL", exception.Operation);
      Assert.Equal("ExchangeRate", exception.EntityName);
    }
  }

  public class InheritedMethodsTests : ExchangeRateRepositoryTests
  {
    [Fact]
    public async Task GetByIdAsync_ShouldWorkCorrectly()
    {
      // arrange
      var rateId = Guid.NewGuid();
      var exchangeRate = new ExchangeRate("USD", "EUR", 0.85m)
      {
        Id = rateId,
        Actual = true,
        ValidFrom = DateTimeOffset.UtcNow,
        ValidTo = null
      };

      await _dbContext.Set<ExchangeRate>().AddAsync(exchangeRate);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetByIdAsync(rateId);

      // assert
      Assert.NotNull(result);
      Assert.Equal(rateId, result.Id);
      Assert.Equal("USD", result.BaseCurrency);
      Assert.Equal("EUR", result.TargetCurrency);
      Assert.Equal(0.85m, result.Rate);
    }

    [Fact]
    public async Task CreateAsync_ShouldWorkCorrectly()
    {
      // arrange
      var exchangeRate = new ExchangeRate("USD", "CAD", 1.25m)
      {
        Actual = true,
        ValidFrom = DateTimeOffset.UtcNow,
        ValidTo = null
      };

      // act
      var result = await _repository.CreateAsync(exchangeRate);

      // assert
      Assert.NotNull(result);
      Assert.Equal(exchangeRate, result);
      Assert.Equal("USD", result.BaseCurrency);
      Assert.Equal("CAD", result.TargetCurrency);
      Assert.Equal(1.25m, result.Rate);

      // Verify it was actually added to the database
      var dbEntity = await _dbContext.Set<ExchangeRate>().FindAsync(exchangeRate.Id);
      Assert.NotNull(dbEntity);
      Assert.Equal("USD", dbEntity.BaseCurrency);
      Assert.Equal("CAD", dbEntity.TargetCurrency);
      Assert.Equal(1.25m, dbEntity.Rate);
    }

    [Fact]
    public async Task DeleteAsync_ShouldWorkCorrectly()
    {
      // arrange
      var exchangeRate = new ExchangeRate("USD", "JPY", 110.5m)
      {
        Actual = true,
        ValidFrom = DateTimeOffset.UtcNow,
        ValidTo = null
      };
      await _dbContext.Set<ExchangeRate>().AddAsync(exchangeRate);
      await _dbContext.SaveChangesAsync();

      // act
      await _repository.DeleteAsync(exchangeRate);
      await _dbContext.SaveChangesAsync(); // Need to save changes to persist the deletion

      // assert
      var dbEntity = await _dbContext.Set<ExchangeRate>().FindAsync(exchangeRate.Id);
      Assert.Null(dbEntity);
    }
  }
}
