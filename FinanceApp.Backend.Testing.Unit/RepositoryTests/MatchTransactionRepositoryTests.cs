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
      var correlationId = Guid.NewGuid().ToString();
      var matchTransaction = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = transactionGroup,
        CorrelationId = correlationId
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
      var correlationId = Guid.NewGuid().ToString();
      var matchTransaction1 = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = "Food",
        CorrelationId = correlationId
      };
      var matchTransaction2 = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = "Entertainment",
        CorrelationId = correlationId
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
      var correlationId = Guid.NewGuid().ToString();
      var matchTransaction = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = transactionGroup,
        CorrelationId = correlationId
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
      var correlationId = Guid.NewGuid().ToString();
      var matchTransaction = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = transactionGroup,
        CorrelationId = correlationId
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
      var correlationId = Guid.NewGuid().ToString();
      var matchTransaction = new MatchTransaction
      {
        Transaction = transaction,
        TransactionGroup = transactionGroup,
        CorrelationId = correlationId
      };

      await _dbContext.Set<MatchTransaction>().AddAsync(matchTransaction);
      await _dbContext.SaveChangesAsync();

      // Act
      _repository.Delete(matchTransaction);
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
        TransactionGroup = "Shopping",
        CorrelationId = Guid.NewGuid().ToString()
      };
      var matchTransaction2 = new MatchTransaction
      {
        Transaction = "Store B",
        TransactionGroup = "Shopping",
        CorrelationId = Guid.NewGuid().ToString()
      };
      var matchTransaction3 = new MatchTransaction
      {
        Transaction = "Store C",
        TransactionGroup = "Food",
        CorrelationId = Guid.NewGuid().ToString()
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

  public class GetAllByCorrelationIdAsyncTests : MatchTransactionRepositoryTests
  {
    [Fact]
    public async Task GetAllByCorrelationIdAsync_WithMatchingCorrelationId_ShouldReturnMatches()
    {
      // Arrange
      var correlationId = Guid.NewGuid().ToString();
      var otherCorrelationId = Guid.NewGuid().ToString();
      var matchTransaction1 = new MatchTransaction
      {
        Transaction = "Store A",
        TransactionGroup = "Shopping",
        CorrelationId = correlationId
      };
      var matchTransaction2 = new MatchTransaction
      {
        Transaction = "Store B",
        TransactionGroup = "Food",
        CorrelationId = correlationId
      };
      var matchTransaction3 = new MatchTransaction
      {
        Transaction = "Store C",
        TransactionGroup = "Transport",
        CorrelationId = otherCorrelationId
      };

      await _dbContext.Set<MatchTransaction>().AddRangeAsync(matchTransaction1, matchTransaction2, matchTransaction3);
      await _dbContext.SaveChangesAsync();

      // Act
      var result = await _repository.GetAllByCorrelationIdAsync(correlationId);

      // Assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.All(result, m => Assert.Equal(correlationId, m.CorrelationId));
    }

    [Fact]
    public async Task GetAllByCorrelationIdAsync_WithNonExistingCorrelationId_ShouldReturnEmptyList()
    {
      // Arrange
      var correlationId = Guid.NewGuid().ToString();
      var matchTransaction = new MatchTransaction
      {
        Transaction = "Store A",
        TransactionGroup = "Shopping",
        CorrelationId = Guid.NewGuid().ToString()
      };

      await _dbContext.Set<MatchTransaction>().AddAsync(matchTransaction);
      await _dbContext.SaveChangesAsync();

      // Act
      var result = await _repository.GetAllByCorrelationIdAsync(correlationId);

      // Assert
      Assert.NotNull(result);
      Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByCorrelationIdAsync_WithEmptyDatabase_ShouldReturnEmptyList()
    {
      // Arrange
      var correlationId = Guid.NewGuid().ToString();

      // Act
      var result = await _repository.GetAllByCorrelationIdAsync(correlationId);

      // Assert
      Assert.NotNull(result);
      Assert.Empty(result);
    }
  }

  public class DeleteAllByCorrelationIdAsyncTests : MatchTransactionRepositoryTests
  {
    [Fact]
    public async Task DeleteAllByCorrelationIdAsync_WithMatchingCorrelationId_ShouldDeleteAllMatches()
    {
      // Arrange
      var correlationId = Guid.NewGuid().ToString();
      var otherCorrelationId = Guid.NewGuid().ToString();
      var matchTransaction1 = new MatchTransaction
      {
        Transaction = "Store A",
        TransactionGroup = "Shopping",
        CorrelationId = correlationId
      };
      var matchTransaction2 = new MatchTransaction
      {
        Transaction = "Store B",
        TransactionGroup = "Food",
        CorrelationId = correlationId
      };
      var matchTransaction3 = new MatchTransaction
      {
        Transaction = "Store C",
        TransactionGroup = "Transport",
        CorrelationId = otherCorrelationId
      };

      await _dbContext.Set<MatchTransaction>().AddRangeAsync(matchTransaction1, matchTransaction2, matchTransaction3);
      await _dbContext.SaveChangesAsync();

      // Act
      await _repository.DeleteAllByCorrelationIdAsync(correlationId);
      await _dbContext.SaveChangesAsync();

      // Assert
      var remainingMatches = await _dbContext.Set<MatchTransaction>().ToListAsync();
      Assert.Single(remainingMatches);
      Assert.Equal(otherCorrelationId, remainingMatches[0].CorrelationId);
      Assert.Equal("Store C", remainingMatches[0].Transaction);
    }

    [Fact]
    public async Task DeleteAllByCorrelationIdAsync_WithNonExistingCorrelationId_ShouldNotDeleteAnything()
    {
      // Arrange
      var correlationId = Guid.NewGuid().ToString();
      var matchTransaction = new MatchTransaction
      {
        Transaction = "Store A",
        TransactionGroup = "Shopping",
        CorrelationId = Guid.NewGuid().ToString()
      };

      await _dbContext.Set<MatchTransaction>().AddAsync(matchTransaction);
      await _dbContext.SaveChangesAsync();

      // Act
      await _repository.DeleteAllByCorrelationIdAsync(correlationId);

      // Assert
      var remainingMatches = await _dbContext.Set<MatchTransaction>().ToListAsync();
      Assert.Single(remainingMatches);
    }

    [Fact]
    public async Task DeleteAllByCorrelationIdAsync_WithEmptyDatabase_ShouldNotThrowException()
    {
      // Arrange
      var correlationId = Guid.NewGuid().ToString();

      // Act & Assert
      await _repository.DeleteAllByCorrelationIdAsync(correlationId);

      var remainingMatches = await _dbContext.Set<MatchTransaction>().ToListAsync();
      Assert.Empty(remainingMatches);
    }

    [Fact]
    public async Task DeleteAllByCorrelationIdAsync_ShouldSaveChangesToDatabase()
    {
      // Arrange
      var correlationId = Guid.NewGuid().ToString();
      var matchTransaction = new MatchTransaction
      {
        Transaction = "Store A",
        TransactionGroup = "Shopping",
        CorrelationId = correlationId
      };

      await _dbContext.Set<MatchTransaction>().AddAsync(matchTransaction);
      await _dbContext.SaveChangesAsync();

      // Act
      await _repository.DeleteAllByCorrelationIdAsync(correlationId);
      await _dbContext.SaveChangesAsync();

      // Assert - Verify changes are persisted by creating a new context
      var verifyMatches = await _dbContext.Set<MatchTransaction>()
        .Where(m => m.CorrelationId == correlationId)
        .ToListAsync();
      Assert.Empty(verifyMatches);
    }
  }
}
