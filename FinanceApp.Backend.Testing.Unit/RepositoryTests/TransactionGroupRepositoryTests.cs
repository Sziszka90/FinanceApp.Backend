using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.FilteredQueryProvider;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;
using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace FinanceApp.Backend.Testing.Unit.RepositoryTests;

public class TransactionGroupRepositoryTests : IDisposable
{
  private readonly FinanceAppSqliteDbContext _dbContext;
  private readonly IFilteredQueryProvider _filteredQueryProvider;
  private readonly TransactionGroupRepository _repository;

  public TransactionGroupRepositoryTests()
  {
    // Create in-memory SQLite database
    var options = new DbContextOptionsBuilder<FinanceAppSqliteDbContext>()
      .UseSqlite("DataSource=:memory:")
      .Options;

    _dbContext = new FinanceAppSqliteDbContext(options);
    _dbContext.Database.OpenConnection();
    _dbContext.Database.EnsureCreated();

    // Create real filtered query provider
    var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    _filteredQueryProvider = new FilteredQueryProvider(_dbContext, httpContextAccessorMock.Object);

    _repository = new TransactionGroupRepository(_dbContext, _filteredQueryProvider);
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

  public class BatchCreateTransactionGroupsAsyncTests : TransactionGroupRepositoryTests
  {
    [Fact]
    public async Task BatchCreateTransactionGroupsAsync_WithValidGroups_ShouldReturnGroups()
    {
      // arrange
      var user = new User("testuser", "test@example.com", "hash", FinanceApp.Backend.Domain.Enums.CurrencyEnum.USD);
      await _dbContext.Set<User>().AddAsync(user);
      await _dbContext.SaveChangesAsync();

      var transactionGroups = new List<TransactionGroup>
      {
        new TransactionGroup("Shopping", "Shopping expenses", null, user)
        {
          Id = Guid.NewGuid()
        },
        new TransactionGroup("Food", "Food and dining", null, user)
        {
          Id = Guid.NewGuid()
        }
      };

      // act
      var result = await _repository.BatchCreateTransactionGroupsAsync(transactionGroups);

      // assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Equal(transactionGroups, result);
      Assert.Equal("Shopping", result[0].Name);
      Assert.Equal("Food", result[1].Name);

      // Verify they were actually added to the database
      var dbGroups = await _dbContext.Set<TransactionGroup>().ToListAsync();
      Assert.Equal(2, dbGroups.Count);
      Assert.Contains(dbGroups, g => g.Name == "Shopping");
      Assert.Contains(dbGroups, g => g.Name == "Food");
    }

    [Fact]
    public async Task BatchCreateTransactionGroupsAsync_WhenExceptionThrown_ShouldThrowDatabaseException()
    {
      // arrange
      var user = new User("testuser", "test@example.com", "hash", FinanceApp.Backend.Domain.Enums.CurrencyEnum.USD);
      var transactionGroups = new List<TransactionGroup>
      {
        new TransactionGroup("Test Group", "Test", null, user)
      };

      // Dispose the context to simulate an error
      _dbContext.Dispose();

      // act & assert
      var exception = await Assert.ThrowsAsync<DatabaseException>(() => _repository.BatchCreateTransactionGroupsAsync(transactionGroups));
      Assert.Equal("BATCH_CREATE", exception.Operation);
      Assert.Equal("TransactionGroup", exception.EntityName);
    }
  }

  public class DeleteAllByUserIdAsyncTests : TransactionGroupRepositoryTests
  {
    [Fact]
    public async Task DeleteAllByUserIdAsync_WhenSuccessful_ShouldDeleteUserTransactionGroups()
    {
      // arrange
      var userId = Guid.NewGuid();
      var user = new User("testuser", "test@example.com", "hash", FinanceApp.Backend.Domain.Enums.CurrencyEnum.USD) { Id = userId };
      await _dbContext.Set<User>().AddAsync(user);

      var transactionGroups = new List<TransactionGroup>
      {
        new TransactionGroup("Shopping", "Shopping expenses", null, user)
        {
          Id = Guid.NewGuid()
        },
        new TransactionGroup("Food", "Food and dining", null, user)
        {
          Id = Guid.NewGuid()
        }
      };

      await _dbContext.Set<TransactionGroup>().AddRangeAsync(transactionGroups);
      await _dbContext.SaveChangesAsync();

      // act
      await _repository.DeleteAllByUserIdAsync(userId);
      await _dbContext.SaveChangesAsync();

      // assert
      var remainingGroups = await _dbContext.Set<TransactionGroup>()
          .Where(tg => tg.User.Id == userId)
          .ToListAsync();

      Assert.Empty(remainingGroups);
    }

    [Fact]
    public async Task DeleteAllByUserIdAsync_WhenExceptionThrown_ShouldThrowDatabaseException()
    {
      // arrange
      var userId = Guid.NewGuid();

      // Dispose the context to simulate an error
      _dbContext.Dispose();

      // act & assert
      var exception = await Assert.ThrowsAsync<DatabaseException>(() => _repository.DeleteAllByUserIdAsync(userId));
      Assert.Equal("DELETE_ALL_BY_USER_ID", exception.Operation);
      Assert.Equal("TransactionGroup", exception.EntityName);
      Assert.Equal(userId.ToString(), exception.EntityId);
    }
  }

  public class GetAllByUserIdAsyncTests : TransactionGroupRepositoryTests
  {
    [Fact]
    public async Task GetAllByUserIdAsync_WithValidUserId_ShouldReturnUserTransactionGroups()
    {
      // arrange
      var userId = Guid.NewGuid();
      var user = new User("testuser", "test@example.com", "hash", FinanceApp.Backend.Domain.Enums.CurrencyEnum.USD) { Id = userId };
      await _dbContext.Set<User>().AddAsync(user);

      var transactionGroups = new List<TransactionGroup>
      {
        new TransactionGroup("Shopping", "Shopping expenses", null, user)
        {
          Id = Guid.NewGuid()
        },
        new TransactionGroup("Food", "Food and dining", null, user)
        {
          Id = Guid.NewGuid()
        }
      };

      await _dbContext.Set<TransactionGroup>().AddRangeAsync(transactionGroups);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetAllByUserIdAsync(userId);

      // assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Contains(result, tg => tg.Name == "Shopping");
      Assert.Contains(result, tg => tg.Name == "Food");
      Assert.All(result, tg => Assert.Equal(userId, tg.User.Id));
    }

    [Fact]
    public async Task GetAllByUserIdAsync_WithNoTracking_ShouldReturnUserTransactionGroups()
    {
      // arrange
      var userId = Guid.NewGuid();
      var user = new User("testuser", "test@example.com", "hash", FinanceApp.Backend.Domain.Enums.CurrencyEnum.USD) { Id = userId };
      await _dbContext.Set<User>().AddAsync(user);

      var transactionGroup = new TransactionGroup("Shopping", "Shopping expenses", null, user)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<TransactionGroup>().AddAsync(transactionGroup);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetAllByUserIdAsync(userId, noTracking: true);

      // assert
      Assert.NotNull(result);
      Assert.Single(result);
      Assert.Equal("Shopping", result[0].Name);
      Assert.Equal(userId, result[0].User.Id);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_WithNonExistentUserId_ShouldReturnEmptyList()
    {
      // arrange
      var userId = Guid.NewGuid();

      // act
      var result = await _repository.GetAllByUserIdAsync(userId);

      // assert
      Assert.NotNull(result);
      Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllByUserIdAsync_WhenExceptionThrown_ShouldThrowDatabaseException()
    {
      // arrange
      var userId = Guid.NewGuid();

      // Dispose the context to simulate an error
      _dbContext.Dispose();

      // act & assert
      var exception = await Assert.ThrowsAsync<DatabaseException>(() => _repository.GetAllByUserIdAsync(userId));
      Assert.Equal("GET_ALL_BY_USER_ID", exception.Operation);
      Assert.Equal("TransactionGroup", exception.EntityName);
      Assert.Equal(userId.ToString(), exception.EntityId);
    }
  }

  public class InheritedMethodsTests : TransactionGroupRepositoryTests
  {
    [Fact]
    public async Task GetByIdAsync_ShouldWorkCorrectly()
    {
      // arrange
      var groupId = Guid.NewGuid();
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", FinanceApp.Backend.Domain.Enums.CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);

      var transactionGroup = new TransactionGroup("Shopping", "Shopping expenses", null, user)
      {
        Id = groupId
      };
      await _dbContext.Set<TransactionGroup>().AddAsync(transactionGroup);
      await _dbContext.SaveChangesAsync();

      // Setup HTTP context with user claims for filtered query provider
      var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
      var httpContext = new DefaultHttpContext();
      var claims = new List<Claim>
      {
        new Claim(ClaimTypes.NameIdentifier, userEmail)
      };
      var identity = new ClaimsIdentity(claims, "test");
      var principal = new ClaimsPrincipal(identity);
      httpContext.User = principal;
      httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

      // Create a new repository with the properly configured HTTP context
      var filteredQueryProvider = new FilteredQueryProvider(_dbContext, httpContextAccessorMock.Object);
      var repository = new TransactionGroupRepository(_dbContext, filteredQueryProvider);

      // act
      var result = await repository.GetByIdAsync(groupId);

      // assert
      Assert.NotNull(result);
      Assert.Equal(groupId, result.Id);
      Assert.Equal("Shopping", result.Name);
      Assert.Equal("Shopping expenses", result.Description);
    }

    [Fact]
    public async Task CreateAsync_ShouldWorkCorrectly()
    {
      // arrange
      var user = new User("testuser", "test@example.com", "hash", FinanceApp.Backend.Domain.Enums.CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);
      await _dbContext.SaveChangesAsync();

      var transactionGroup = new TransactionGroup("New Group", "A new transaction group", null, user);

      // act
      var result = await _repository.CreateAsync(transactionGroup);
      await _dbContext.SaveChangesAsync();

      // assert
      Assert.NotNull(result);
      Assert.Equal(transactionGroup, result);
      Assert.Equal("New Group", result.Name);
      Assert.Equal("A new transaction group", result.Description);

      // Verify it was added to the database
      var savedGroup = await _dbContext.Set<TransactionGroup>().FindAsync(result.Id);
      Assert.NotNull(savedGroup);
      Assert.Equal("New Group", savedGroup.Name);
    }

    [Fact]
    public async Task DeleteAsync_ShouldWorkCorrectly()
    {
      // arrange
      var user = new User("testuser", "test@example.com", "hash", FinanceApp.Backend.Domain.Enums.CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);

      var transactionGroup = new TransactionGroup("Group to Delete", "This group will be deleted", null, user);
      await _dbContext.Set<TransactionGroup>().AddAsync(transactionGroup);
      await _dbContext.SaveChangesAsync();

      // act
      await _repository.DeleteAsync(transactionGroup);
      await _dbContext.SaveChangesAsync();

      // assert
      var deletedGroup = await _dbContext.Set<TransactionGroup>().FindAsync(transactionGroup.Id);
      Assert.Null(deletedGroup);
    }
  }
}
