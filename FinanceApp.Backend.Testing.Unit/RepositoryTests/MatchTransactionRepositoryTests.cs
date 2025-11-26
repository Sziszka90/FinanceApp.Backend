using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.FilteredQueryProvider;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;
using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.RepositoryTests;

public class MatchTransactionRepositoryTests : IDisposable
{
  private readonly FinanceAppSqliteDbContext _dbContext;
  private readonly IFilteredQueryProvider _filteredQueryProvider;
  private readonly MatchTransactionRepository _repository;

  public MatchTransactionRepositoryTests()
  {
    var options = new DbContextOptionsBuilder<FinanceAppSqliteDbContext>()
      .UseSqlite("DataSource=:memory:")
      .Options;

    _dbContext = new FinanceAppSqliteDbContext(options);
    _dbContext.Database.OpenConnection();
    _dbContext.Database.EnsureCreated();

    var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    _filteredQueryProvider = new FilteredQueryProvider(_dbContext, httpContextAccessorMock.Object);

    _repository = new MatchTransactionRepository(_dbContext, _filteredQueryProvider);
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

  public class GetByTransactionAndGroupAsyncTests : MatchTransactionRepositoryTests
  {
    [Fact]
    public async Task GetByIdAsync_WithExistingMatch_ShouldReturnMatch()
    {
      // Arrange
      var transaction = "Grocery Store";
      var transactionGroup = "Food";
      var matchTransaction = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = transactionGroup
      };

      await _dbContext.Set<MatchTransaction>().AddAsync(matchTransaction);
      await _dbContext.SaveChangesAsync();

      // Act
      var result = await _dbContext.Set<MatchTransaction>()
        .FirstOrDefaultAsync(m => m.Transaction == transaction && m.TransactionGroup == transactionGroup);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(transaction, result.Transaction);
      Assert.Equal(transactionGroup, result.TransactionGroup);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingMatch_ShouldReturnNull()
    {
      // Arrange
      var transaction = "NonExistent";
      var transactionGroup = "NonExistent";

      // Act
      var result = await _dbContext.Set<MatchTransaction>()
        .FirstOrDefaultAsync(m => m.Transaction == transaction && m.TransactionGroup == transactionGroup);

      // Assert
      Assert.Null(result);
    }
  }

  public class GetByTransactionAsyncTests : MatchTransactionRepositoryTests
  {
    [Fact]
    public async Task GetAllAsync_WithExistingMatches_ShouldReturnAllMatches()
    {
      // Arrange
      var transaction = "Coffee Shop";
      var matchTransaction1 = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = "Food"
      };
      var matchTransaction2 = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = "Entertainment"
      };

      await _dbContext.Set<MatchTransaction>().AddRangeAsync(matchTransaction1, matchTransaction2);
      await _dbContext.SaveChangesAsync();

      // Act
      var result = await _repository.GetAllAsync();
      var filtered = result.Where(m => m.Transaction == transaction).ToList();

      // Assert
      Assert.NotNull(filtered);
      Assert.Equal(2, filtered.Count);
      Assert.All(filtered, m => Assert.Equal(transaction, m.Transaction));
    }

    [Fact]
    public async Task GetAllAsync_WithNonExistingTransaction_ShouldReturnEmptyList()
    {
      // Arrange
      var transaction = "NonExistent";

      // Act
      var result = await _repository.GetAllAsync();
      var filtered = result.Where(m => m.Transaction == transaction).ToList();

      // Assert
      Assert.NotNull(filtered);
      Assert.Empty(filtered);
    }
  }

  public class ExistsAsyncTests : MatchTransactionRepositoryTests
  {
    [Fact]
    public async Task AnyAsync_WithExistingMatch_ShouldReturnTrue()
    {
      // Arrange
      var transaction = "Restaurant";
      var transactionGroup = "Food";
      var matchTransaction = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = transactionGroup
      };

      await _dbContext.Set<MatchTransaction>().AddAsync(matchTransaction);
      await _dbContext.SaveChangesAsync();

      // Act
      var result = await _dbContext.Set<MatchTransaction>()
        .AnyAsync(m => m.Transaction == transaction && m.TransactionGroup == transactionGroup);

      // Assert
      Assert.True(result);
    }

    [Fact]
    public async Task AnyAsync_WithNonExistingMatch_ShouldReturnFalse()
    {
      // Arrange
      var transaction = "NonExistent";
      var transactionGroup = "NonExistent";

      // Act
      var result = await _dbContext.Set<MatchTransaction>()
        .AnyAsync(m => m.Transaction == transaction && m.TransactionGroup == transactionGroup);

      // Assert
      Assert.False(result);
    }
  }

  public class CreateAsyncTests : MatchTransactionRepositoryTests
  {
    [Fact]
    public async Task CreateAsync_WithValidMatch_ShouldAddToDatabase()
    {
      // Arrange
      var transaction = "Gas Station";
      var transactionGroup = "Transport";
      var matchTransaction = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = transactionGroup
      };

      // Act
      var result = await _repository.CreateAsync(matchTransaction);
      await _dbContext.SaveChangesAsync();

      // Assert
      Assert.NotNull(result);
      var savedMatch = await _dbContext.Set<MatchTransaction>()
        .FirstOrDefaultAsync(m => m.Transaction == transaction && m.TransactionGroup == transactionGroup);
      Assert.NotNull(savedMatch);
      Assert.Equal(transaction, savedMatch.Transaction);
      Assert.Equal(transactionGroup, savedMatch.TransactionGroup);
    }
  }

  public class DeleteAsyncTests : MatchTransactionRepositoryTests
  {
    [Fact]
    public async Task DeleteAsync_WithExistingMatch_ShouldRemoveFromDatabase()
    {
      // Arrange
      var transaction = "Pharmacy";
      var transactionGroup = "Health";
      var matchTransaction = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = transactionGroup
      };

      await _dbContext.Set<MatchTransaction>().AddAsync(matchTransaction);
      await _dbContext.SaveChangesAsync();

      // Act
      await _repository.DeleteAsync(matchTransaction);
      await _dbContext.SaveChangesAsync();

      // Assert
      var deletedMatch = await _dbContext.Set<MatchTransaction>()
        .FirstOrDefaultAsync(m => m.Transaction == transaction && m.TransactionGroup == transactionGroup);
      Assert.Null(deletedMatch);
    }
  }

  public class GetAllAsyncTests : MatchTransactionRepositoryTests
  {
    [Fact]
    public async Task GetAllAsync_WithMultipleMatches_ShouldReturnAllMatches()
    {
      // Arrange
      var matchTransaction1 = new MatchTransaction
      {
        Transaction = "Store A",
        TransactionGroup = "Shopping"
      };
      var matchTransaction2 = new MatchTransaction
      {
        Transaction = "Store B",
        TransactionGroup = "Shopping"
      };
      var matchTransaction3 = new MatchTransaction
      {
        Transaction = "Store C",
        TransactionGroup = "Food"
      };

      await _dbContext.Set<MatchTransaction>().AddRangeAsync(matchTransaction1, matchTransaction2, matchTransaction3);
      await _dbContext.SaveChangesAsync();

      // Act
      var result = await _repository.GetAllAsync();

      // Assert
      Assert.NotNull(result);
      Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_WithNoMatches_ShouldReturnEmptyList()
    {
      // Act
      var result = await _repository.GetAllAsync();

      // Assert
      Assert.NotNull(result);
      Assert.Empty(result);
    }
  }
}
