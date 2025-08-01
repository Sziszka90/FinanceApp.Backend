using System.Security.Claims;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.CreateTransaction;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class CreateTransactionTests : TestBase
{
  private readonly Mock<ILogger<CreateTransactionCommandHandler>> _loggerMock;
  private readonly CreateTransactionCommandHandler _handler;

  public CreateTransactionTests()
  {
    _loggerMock = CreateLoggerMock<CreateTransactionCommandHandler>();
    _handler = new CreateTransactionCommandHandler(
        _loggerMock.Object,
        HttpContextAccessorMock.Object,
        Mapper,
        TransactionRepositoryMock.Object,
        UserRepositoryMock.Object,
        TransactionGroupRepositoryMock.Object,
        UnitOfWorkMock.Object
    );
  }

  [Fact]
  public async Task CreateTransaction_ValidRequest_CreatesTransaction()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);

    var transaction = new Transaction(
      "Test Transaction",
      "Description",
      TransactionTypeEnum.Income,
      new Money()
      {
        Amount = 100,
        Currency = CurrencyEnum.USD
      },
      new TransactionGroup("Test Group", "Description", "", user),
      DateTime.UtcNow,
      user
    );

    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email, false, It.IsAny<CancellationToken>())).ReturnsAsync(user);
    TransactionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
    var createDto = new CreateTransactionDto();
    var command = new CreateTransactionCommand(createDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(user.Email, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CreateTransaction_UserNotFound_ReturnsFailure()
  {
    // arrange
    var userEmail = "notfound@example.com";
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userEmail) };
    var identity = new ClaimsIdentity(claims, "TestAuthType");
    var principal = new ClaimsPrincipal(identity);
    var httpContext = new DefaultHttpContext { User = principal };
    HttpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);
    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(userEmail, false, It.IsAny<CancellationToken>())).ReturnsAsync((User)null!);
    var createDto = new CreateTransactionDto();
    var command = new CreateTransactionCommand(createDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    UserRepositoryMock.Verify(x => x.GetUserByEmailAsync(userEmail, false, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
