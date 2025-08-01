using FinanceApp.Backend.Application.UserApi.UserQueries.GetUserById;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.UserTests.Queries;

public class GetUserByIdQueryTests : TestBase
{
  private readonly Mock<ILogger<GetUserByIdQueryHandler>> _loggerMock;
  private readonly GetUserByIdQueryHandler _handler;

  public GetUserByIdQueryTests()
  {
    _loggerMock = CreateLoggerMock<GetUserByIdQueryHandler>();
    _handler = new GetUserByIdQueryHandler(
        _loggerMock.Object,
        Mapper,
        UserRepositoryMock.Object
    );
  }

  [Fact]
  public async Task QueryUser_ValidId_ReturnsUser()
  {
    // arrange
    var userId = Guid.NewGuid();
    var user = new User(userId, "testuser", "test@example.com", true, "hash", Domain.Enums.CurrencyEnum.USD);
    UserRepositoryMock.Setup(x => x.GetByIdAsync(userId, true, It.IsAny<CancellationToken>())).ReturnsAsync(user);

    var query = new GetUserByIdQuery(userId, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(userId, result.Data.Id);
    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, true, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task QueryUser_UserNotFound_ReturnsFailure()
  {
    // arrange
    var userId = Guid.NewGuid();
    var query = new GetUserByIdQuery(userId, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.Null(result.Data);
    UserRepositoryMock.Verify(x => x.GetByIdAsync(userId, true, It.IsAny<CancellationToken>()), Times.Once);
  }
}
