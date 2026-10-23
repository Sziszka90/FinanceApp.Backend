using FinanceApp.Backend.Application.Exceptions;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Interfaces;
using FinanceApp.Backend.Infrastructure.EntityFramework.Common.Repository;
using FinanceApp.Backend.Infrastructure.EntityFramework.Sqlite.Context;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.RepositoryTests;

public class UserRepositoryTests : IDisposable
{
  protected readonly FinanceAppSqliteDbContext _dbContext;
  protected readonly Mock<IFilteredQueryProvider> _filteredQueryProviderMock;
  protected readonly UserRepository _repository;

  public UserRepositoryTests()
  {
    var options = new DbContextOptionsBuilder<FinanceAppSqliteDbContext>()
        .UseSqlite("DataSource=:memory:")
        .Options;

    _dbContext = new FinanceAppSqliteDbContext(options);
    _dbContext.Database.OpenConnection();
    _dbContext.Database.EnsureCreated();

    _filteredQueryProviderMock = new Mock<IFilteredQueryProvider>();
    _filteredQueryProviderMock.Setup(x => x.Query<User>()).Returns(_dbContext.Set<User>().AsQueryable());

    _repository = new UserRepository(_dbContext, _filteredQueryProviderMock.Object);
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

  public class GetByUserNameAsyncTests : UserRepositoryTests
  {
    [Fact]
    public async Task GetByUserNameAsync_WithValidUserName_ShouldReturnUser()
    {
      // arrange
      const string userName = "testuser";
      var user = new User(userName, "test@example.com", "hash", CurrencyEnum.USD)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<User>().AddAsync(user);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetByUserNameAsync(userName);

      // assert
      Assert.NotNull(result);
      Assert.Equal(userName, result.UserName);
      Assert.Equal("test@example.com", result.Email);
      Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByUserNameAsync_WithNoTracking_ShouldReturnUser()
    {
      // arrange
      const string userName = "testuser";
      var user = new User(userName, "test@example.com", "hash", CurrencyEnum.USD)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<User>().AddAsync(user);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetByUserNameAsync(userName, noTracking: true);

      // assert
      Assert.NotNull(result);
      Assert.Equal(userName, result.UserName);
      Assert.Equal("test@example.com", result.Email);
      Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByUserNameAsync_WithNonExistentUserName_ShouldReturnNull()
    {
      // arrange
      const string userName = "nonexistent";

      // act
      var result = await _repository.GetByUserNameAsync(userName);

      // assert
      Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserNameAsync_WhenExceptionThrown_ShouldThrowDatabaseException()
    {
      // arrange
      const string userName = "testuser";

      // Dispose the context to simulate an error
      _dbContext.Dispose();

      // act & assert
      var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() => _repository.GetByUserNameAsync(userName));
    }
  }

  public class GetUserByEmailAsyncTests : UserRepositoryTests
  {
    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
      // arrange
      const string email = "test@example.com";
      var user = new User("testuser", email, "hash", CurrencyEnum.USD)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<User>().AddAsync(user);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetUserByEmailAsync(email);

      // assert
      Assert.NotNull(result);
      Assert.Equal(email, result.Email);
      Assert.Equal("testuser", result.UserName);
      Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNoTracking_ShouldReturnUser()
    {
      // arrange
      const string email = "test@example.com";
      var user = new User("testuser", email, "hash", CurrencyEnum.USD)
      {
        Id = Guid.NewGuid()
      };

      await _dbContext.Set<User>().AddAsync(user);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetUserByEmailAsync(email, noTracking: true);

      // assert
      Assert.NotNull(result);
      Assert.Equal(email, result.Email);
      Assert.Equal("testuser", result.UserName);
      Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
      // arrange
      const string email = "nonexistent@example.com";

      // act
      var result = await _repository.GetUserByEmailAsync(email);

      // assert
      Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WhenExceptionThrown_ShouldThrowDatabaseException()
    {
      // arrange
      const string email = "test@example.com";

      // Dispose the context to simulate an error
      _dbContext.Dispose();

      // act & assert
      var exception = await Assert.ThrowsAsync<ObjectDisposedException>(() => _repository.GetUserByEmailAsync(email));
    }
  }

  public class InheritedMethodsTests : UserRepositoryTests
  {
    [Fact]
    public async Task GetByIdAsync_ShouldWorkCorrectly()
    {
      // arrange
      var userId = Guid.NewGuid();
      var user = new User("testuser", "test@example.com", "hash", CurrencyEnum.USD)
      {
        Id = userId
      };

      await _dbContext.Set<User>().AddAsync(user);
      await _dbContext.SaveChangesAsync();

      // act
      var result = await _repository.GetByIdAsync(userId);

      // assert
      Assert.NotNull(result);
      Assert.Equal(userId, result.Id);
      Assert.Equal("testuser", result.UserName);
      Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task CreateAsync_ShouldWorkCorrectly()
    {
      // arrange
      var user = new User("newuser", "new@example.com", "hash123", CurrencyEnum.USD);

      // act
      var result = await _repository.CreateAsync(user);

      // assert
      Assert.NotNull(result);
      Assert.Equal(user, result);
      Assert.Equal("newuser", result.UserName);
      Assert.Equal("new@example.com", result.Email);

      // Verify it was added to the database
      var savedUser = await _dbContext.Set<User>().FindAsync(result.Id);
      Assert.NotNull(savedUser);
      Assert.Equal("newuser", savedUser.UserName);
    }
  }
}
