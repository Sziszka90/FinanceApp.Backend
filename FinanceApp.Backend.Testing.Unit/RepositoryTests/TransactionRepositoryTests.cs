using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.FilteredQueryProvider;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Services.Abstraction;
using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace FinanceApp.Backend.Testing.Unit.RepositoryTests;

public class TransactionRepositoryTests : IDisposable
{
  protected readonly FinanceAppSqliteDbContext _dbContext;
  protected readonly IFilteredQueryProvider _filteredQueryProvider;
  protected readonly Mock<ISqlQueryBuilder> _sqlQueryBuilderMock;
  protected readonly Mock<IDatabaseCommandService> _databaseCommandServiceMock;
  protected readonly TransactionRepository _repository;

  public TransactionRepositoryTests()
  {
    var options = new DbContextOptionsBuilder<FinanceAppSqliteDbContext>()
        .UseSqlite("DataSource=:memory:")
        .Options;

    _dbContext = new FinanceAppSqliteDbContext(options);
    _dbContext.Database.OpenConnection();
    _dbContext.Database.EnsureCreated();

    // Create real filtered query provider
    var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    _filteredQueryProvider = new FilteredQueryProvider(_dbContext, httpContextAccessorMock.Object);

    // Create mocks for additional dependencies
    _sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
    _databaseCommandServiceMock = new Mock<IDatabaseCommandService>();

    _repository = new TransactionRepository(_dbContext, _filteredQueryProvider, _sqlQueryBuilderMock.Object, _databaseCommandServiceMock.Object);
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

  public class TransactionGroupUsedAsyncTests : TransactionRepositoryTests
  {
    [Fact]
    public async Task TransactionGroupUsedAsync_WithUsedGroup_ShouldReturnTrue()
    {
      // arrange
      var groupId = Guid.NewGuid();
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);

      var transactionGroup = new TransactionGroup("Test Group", null, null, user) { Id = groupId };
      await _dbContext.Set<TransactionGroup>().AddAsync(transactionGroup);

      var money = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var transaction = new Transaction(
        "Test Transaction",
        null,
        TransactionTypeEnum.Expense,
        money,
        transactionGroup,
        DateTime.UtcNow,
        user)
      {
        Id = Guid.NewGuid()
      };
      await _dbContext.Set<Transaction>().AddAsync(transaction);
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

      var repository = new TransactionRepository(
        _dbContext,
        filteredQueryProvider,
        _sqlQueryBuilderMock.Object,
        _databaseCommandServiceMock.Object
      );

      // act
      var result = await repository.TransactionGroupUsedAsync(groupId);

      // assert
      Assert.True(result);
    }

    [Fact]
    public async Task TransactionGroupUsedAsync_WithUnusedGroup_ShouldReturnFalse()
    {
      // arrange
      var groupId = Guid.NewGuid();

      // act
      var result = await _repository.TransactionGroupUsedAsync(groupId);

      // assert
      Assert.False(result);
    }

    [Fact]
    public async Task TransactionGroupUsedAsync_WhenExceptionThrown_ShouldThrowDatabaseException()
    {
      // arrange
      var groupId = Guid.NewGuid();

      // Dispose the context to simulate an error
      _dbContext.Dispose();

      // act & assert
      var exception = await Assert.ThrowsAsync<DatabaseException>(() => _repository.TransactionGroupUsedAsync(groupId));
      Assert.Equal("TRANSACTION_GROUP_USED_CHECK", exception.Operation);
      Assert.Equal("Transaction", exception.EntityName);
      Assert.Equal(groupId.ToString(), exception.EntityId);
    }
  }

  public class GetAllByFilterAsyncTests : TransactionRepositoryTests
  {
    [Fact]
    public async Task GetAllByFilterAsync_WithTransactionGroupNameFilter_ShouldReturnFilteredTransactions()
    {
      // arrange
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);

      var transactionGroup = new TransactionGroup("Shopping", null, null, user) { Id = Guid.NewGuid() };
      var incomeGroup = new TransactionGroup("Income", null, null, user) { Id = Guid.NewGuid() };
      await _dbContext.Set<TransactionGroup>().AddRangeAsync(transactionGroup, incomeGroup);

      var money1 = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var money2 = new Money { Amount = 5000, Currency = CurrencyEnum.USD };

      var transaction1 = new Transaction(
        "Amazon",
        null,
        TransactionTypeEnum.Expense,
        money1,
        transactionGroup,
        DateTime.UtcNow,
        user)
      {
        Id = Guid.NewGuid()
      };

      var transaction2 = new Transaction(
        "Salary",
        null,
        TransactionTypeEnum.Income,
        money2,
        incomeGroup,
        DateTime.UtcNow,
        user)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<Transaction>().AddRangeAsync(transaction1, transaction2);
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
      var sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
      var databaseCommandServiceMock = new Mock<IDatabaseCommandService>();
      var repository = new TransactionRepository(_dbContext, filteredQueryProvider, sqlQueryBuilderMock.Object, databaseCommandServiceMock.Object);

      var filter = new TransactionFilter { TransactionGroupName = "Shopping" };

      // act
      var result = await repository.GetAllByFilterAsync(filter);

      // assert
      Assert.NotNull(result);
      Assert.Single(result);
      Assert.Equal("Amazon", result[0].Name);
      Assert.Equal("Shopping", result[0].TransactionGroup?.Name);
    }

    [Fact]
    public async Task GetAllByFilterAsync_WithOrderByTransactionName_ShouldOrderCorrectly()
    {
      // arrange
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);

      var money1 = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var money2 = new Money { Amount = 200, Currency = CurrencyEnum.USD };

      var transaction1 = new Transaction(
        "Zebra Store",
        null,
        TransactionTypeEnum.Expense,
        money1,
        null,
        DateTime.UtcNow,
        user)
      {
        Id = Guid.NewGuid()
      };

      var transaction2 = new Transaction(
        "Apple Store",
        null,
        TransactionTypeEnum.Expense,
        money2,
        null,
        DateTime.UtcNow,
        user)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<Transaction>().AddRangeAsync(transaction1, transaction2);
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
      var sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
      var databaseCommandServiceMock = new Mock<IDatabaseCommandService>();
      var repository = new TransactionRepository(_dbContext, filteredQueryProvider, sqlQueryBuilderMock.Object, databaseCommandServiceMock.Object);

      var filter = new TransactionFilter { OrderBy = "TransactionName", Ascending = true };

      // act
      var result = await repository.GetAllByFilterAsync(filter);

      // assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Contains(result, t => t.Name == "Apple Store");
      Assert.Contains(result, t => t.Name == "Zebra Store");
      // With real database, we can verify the order
      Assert.Equal("Apple Store", result[0].Name);
      Assert.Equal("Zebra Store", result[1].Name);
    }

    [Fact]
    public async Task GetAllByFilterAsync_WithNoTracking_ShouldReturnTransactions()
    {
      // arrange
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);

      var money = new Money { Amount = 100, Currency = CurrencyEnum.USD };

      var transaction = new Transaction(
        "Test",
        null,
        TransactionTypeEnum.Expense,
        money,
        null,
        DateTime.UtcNow,
        user)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<Transaction>().AddAsync(transaction);
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
      var sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
      var databaseCommandServiceMock = new Mock<IDatabaseCommandService>();
      var repository = new TransactionRepository(_dbContext, filteredQueryProvider, sqlQueryBuilderMock.Object, databaseCommandServiceMock.Object);

      var filter = new TransactionFilter();

      // act
      var result = await repository.GetAllByFilterAsync(filter, noTracking: true);

      // assert
      Assert.NotNull(result);
      Assert.Single(result);
      Assert.Equal("Test", result[0].Name);
    }

    [Fact]
    public async Task GetAllByFilterAsync_WhenExceptionThrown_ShouldThrowDatabaseException()
    {
      // arrange
      var filter = new TransactionFilter();

      // Dispose the context to simulate an error
      _dbContext.Dispose();

      // act & assert
      var exception = await Assert.ThrowsAsync<DatabaseException>(() => _repository.GetAllByFilterAsync(filter));
      Assert.Equal("GET_ALL_BY_FILTER", exception.Operation);
      Assert.Equal("Transaction", exception.EntityName);
    }
  }

  public class BatchCreateTransactionsAsyncTests : TransactionRepositoryTests
  {
    [Fact]
    public async Task BatchCreateTransactionsAsync_WithValidTransactions_ShouldReturnTransactions()
    {
      // arrange
      var user = new User("testuser", "test@example.com", "hash", CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);
      await _dbContext.SaveChangesAsync();

      var money1 = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var money2 = new Money { Amount = 200, Currency = CurrencyEnum.USD };

      var transactions = new List<Transaction>
      {
        new Transaction(
          "Transaction1",
          null,
          TransactionTypeEnum.Expense,
          money1,
          null,
          DateTime.UtcNow,
          user)
        {
          Id = Guid.NewGuid()
        },
        new Transaction(
          "Transaction2",
          null,
          TransactionTypeEnum.Income,
          money2,
          null,
          DateTime.UtcNow,
          user)
        {
          Id = Guid.NewGuid()
        }
      };

      // act
      var result = await _repository.BatchCreateTransactionsAsync(transactions);
      await _dbContext.SaveChangesAsync();

      // assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Equal(transactions, result);

      // Verify transactions were actually saved to database
      var savedTransactions = await _dbContext.Set<Transaction>().ToListAsync();
      Assert.Equal(2, savedTransactions.Count);
    }

    [Fact]
    public async Task BatchCreateTransactionsAsync_WhenExceptionThrown_ShouldThrowDatabaseException()
    {
      // arrange
      var user = new User("testuser", "test@example.com", "hash", CurrencyEnum.USD);
      var money = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var transactions = new List<Transaction>
      {
        new Transaction("Test", null, TransactionTypeEnum.Expense, money, null, DateTime.UtcNow, user)
      };

      // Dispose the context to simulate an error
      _dbContext.Dispose();

      // act & assert
      var exception = await Assert.ThrowsAsync<DatabaseException>(() => _repository.BatchCreateTransactionsAsync(transactions));
      Assert.Equal("BATCH_CREATE", exception.Operation);
      Assert.Equal("Transaction", exception.EntityName);
    }
  }

  public class DeleteAllByUserIdAsyncTests : TransactionRepositoryTests
  {
    [Fact]
    public async Task DeleteAllByUserIdAsync_WithValidUserId_ShouldRemoveUserTransactions()
    {
      // arrange
      var userId = Guid.NewGuid();
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", CurrencyEnum.USD) { Id = userId };
      await _dbContext.Set<User>().AddAsync(user);

      var money1 = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var money2 = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var transactions = new List<Transaction>
      {
        new Transaction(
          "Transaction1",
          null,
          TransactionTypeEnum.Expense,
          money1,
          null,
          DateTime.UtcNow,
          user)
        {
          Id = Guid.NewGuid()
        },
        new Transaction(
          "Transaction2",
          null,
          TransactionTypeEnum.Income,
          money2,
          null,
          DateTime.UtcNow,
          user)
        {
          Id = Guid.NewGuid()
        }
      };

      await _dbContext.Set<Transaction>().AddRangeAsync(transactions);
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
      var sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
      var databaseCommandServiceMock = new Mock<IDatabaseCommandService>();
      var repository = new TransactionRepository(_dbContext, filteredQueryProvider, sqlQueryBuilderMock.Object, databaseCommandServiceMock.Object);

      // act
      await repository.DeleteAllByUserIdAsync(userId);
      await _dbContext.SaveChangesAsync();

      // assert
      var remainingTransactions = await _dbContext.Set<Transaction>()
          .Where(t => t.User.Id == userId)
          .ToListAsync();

      Assert.Empty(remainingTransactions);
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
      Assert.Equal("Transaction", exception.EntityName);
      Assert.Equal(userId.ToString(), exception.EntityId);
    }
  }

  public class GetAllByUserIdAsyncTests : TransactionRepositoryTests
  {
    [Fact]
    public async Task GetAllByUserIdAsync_WithValidUserId_ShouldReturnUserTransactions()
    {
      // arrange
      var userId = Guid.NewGuid();
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", CurrencyEnum.USD) { Id = userId };
      await _dbContext.Set<User>().AddAsync(user);

      var transactions = new List<Transaction>
      {
        new Transaction(
          "Transaction1",
          null,
          TransactionTypeEnum.Expense,
          new Money { Amount = 100, Currency = CurrencyEnum.USD },
          null,
          DateTime.UtcNow,
          user)
        {
          Id = Guid.NewGuid()
        },
        new Transaction(
          "Transaction2",
          null,
          TransactionTypeEnum.Income,
          new Money { Amount = 200, Currency = CurrencyEnum.USD },
          null,
          DateTime.UtcNow,
          user)
        {
          Id = Guid.NewGuid()
        }
      };

      await _dbContext.Set<Transaction>().AddRangeAsync(transactions);
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
      var sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
      var databaseCommandServiceMock = new Mock<IDatabaseCommandService>();
      var repository = new TransactionRepository(_dbContext, filteredQueryProvider, sqlQueryBuilderMock.Object, databaseCommandServiceMock.Object);

      // act
      var result = await repository.GetAllByUserIdAsync(userId);

      // assert
      Assert.NotNull(result);
      Assert.Equal(2, result.Count);
      Assert.Contains(result, t => t.Name == "Transaction1");
      Assert.Contains(result, t => t.Name == "Transaction2");
      Assert.All(result, t => Assert.Equal(userId, t.User.Id));
    }

    [Fact]
    public async Task GetAllByUserIdAsync_WithNoTracking_ShouldReturnUserTransactions()
    {
      // arrange
      var userId = Guid.NewGuid();
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", CurrencyEnum.USD) { Id = userId };
      await _dbContext.Set<User>().AddAsync(user);

      var money = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var transaction = new Transaction(
        "Transaction1",
        null,
        TransactionTypeEnum.Expense,
        money,
        null,
        DateTime.UtcNow,
        user)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<Transaction>().AddAsync(transaction);
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
      var sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
      var databaseCommandServiceMock = new Mock<IDatabaseCommandService>();
      var repository = new TransactionRepository(_dbContext, filteredQueryProvider, sqlQueryBuilderMock.Object, databaseCommandServiceMock.Object);

      // act
      var result = await repository.GetAllByUserIdAsync(userId, noTracking: true);

      // assert
      Assert.NotNull(result);
      Assert.Single(result);
      Assert.Equal("Transaction1", result[0].Name);
      Assert.Equal(userId, result[0].User.Id);
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
      Assert.Equal("Transaction", exception.EntityName);
      Assert.Equal(userId.ToString(), exception.EntityId);
    }
  }

  public class OverriddenMethodsTests : TransactionRepositoryTests
  {
    [Fact]
    public async Task GetAllAsync_ShouldIncludeTransactionGroupAndUser()
    {
      // arrange
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);

      var transactionGroup = new TransactionGroup("Shopping", null, null, user) { Id = Guid.NewGuid() };
      await _dbContext.Set<TransactionGroup>().AddAsync(transactionGroup);

      var money = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var transaction = new Transaction(
        "Amazon",
        null,
        TransactionTypeEnum.Expense,
        money,
        transactionGroup,
        DateTime.UtcNow,
        user)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<Transaction>().AddAsync(transaction);
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
      var sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
      var databaseCommandServiceMock = new Mock<IDatabaseCommandService>();
      var repository = new TransactionRepository(_dbContext, filteredQueryProvider, sqlQueryBuilderMock.Object, databaseCommandServiceMock.Object);

      // act
      var result = await repository.GetAllAsync();

      // assert
      Assert.NotNull(result);
      Assert.Single(result);
      Assert.Equal("Amazon", result[0].Name);
      Assert.NotNull(result[0].TransactionGroup);
      Assert.Equal("Shopping", result[0].TransactionGroup!.Name);
      Assert.NotNull(result[0].User);
      Assert.Equal(user.Id, result[0].User.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeTransactionGroup()
    {
      // arrange
      var transactionId = Guid.NewGuid();
      var userEmail = "test@example.com";
      var user = new User("testuser", userEmail, "hash", CurrencyEnum.USD) { Id = Guid.NewGuid() };
      await _dbContext.Set<User>().AddAsync(user);

      var transactionGroup = new TransactionGroup("Shopping", null, null, user) { Id = Guid.NewGuid() };
      await _dbContext.Set<TransactionGroup>().AddAsync(transactionGroup);

      var money = new Money { Amount = 100, Currency = CurrencyEnum.USD };
      var transaction = new Transaction(
        "Amazon",
        null,
        TransactionTypeEnum.Expense,
        money,
        transactionGroup,
        DateTime.UtcNow,
        user)
      {
        Id = transactionId
      };

      await _dbContext.Set<Transaction>().AddAsync(transaction);
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
      var sqlQueryBuilderMock = new Mock<ISqlQueryBuilder>();
      var databaseCommandServiceMock = new Mock<IDatabaseCommandService>();
      var repository = new TransactionRepository(_dbContext, filteredQueryProvider, sqlQueryBuilderMock.Object, databaseCommandServiceMock.Object);

      // act
      var result = await repository.GetByIdAsync(transactionId);

      // assert
      Assert.NotNull(result);
      Assert.Equal("Amazon", result.Name);
      Assert.NotNull(result.TransactionGroup);
      Assert.Equal("Shopping", result.TransactionGroup.Name);
      Assert.Equal(transactionId, result.Id);
    }
  }
}
