using System.Security.Claims;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.UserApi.UserQueries.GetActiveUser;
using FinanceApp.Backend.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.UserTests.Queries;

public class GetActiveUserQueryTests : TestBase
{
  private readonly Mock<ILogger<GetActiveUserQueryHandler>> _loggerMock;
  private readonly GetActiveUserQueryHandler _handler;

  public GetActiveUserQueryTests()
  {
    _loggerMock = CreateLoggerMock<GetActiveUserQueryHandler>();
    _handler = new GetActiveUserQueryHandler(
        _loggerMock.Object,
        Mapper,
        UserRepositoryMock.Object,
        HttpContextAccessorMock.Object
    );
  }

  [Fact]
  public async Task QueryUser_ValidUser_ReturnsUser()
  {
    // arrange
    var userEmail = "test@example.com";
    var user = new User(null, "testuser", userEmail, true, "hash", Domain.Enums.CurrencyEnum.USD);
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userEmail) };
    var identity = new ClaimsIdentity(claims, "TestAuthType");
    var principal = new ClaimsPrincipal(identity);
    var httpContext = new DefaultHttpContext { User = principal };
    HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
    UserRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), true, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<User> { user });

    var query = new GetActiveUserQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(userEmail, result.Data.Email);
    UserRepositoryMock.Verify(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), true, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task QueryUser_UserNotFound_ReturnsFailure()
  {
    // arrange
    var userEmail = "notfound@example.com";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userEmail) };
    var identity = new ClaimsIdentity(claims, "TestAuthType");
    var principal = new ClaimsPrincipal(identity);
    var httpContext = new DefaultHttpContext { User = principal };
    HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
    UserRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), true, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<User>());

    var query = new GetActiveUserQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    Assert.Null(result.Data);
    UserRepositoryMock.Verify(x => x.GetQueryAsync(It.IsAny<QueryCriteria<User>>(), true, It.IsAny<CancellationToken>()), Times.Once);
  }
}
