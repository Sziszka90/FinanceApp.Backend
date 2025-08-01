using FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetTransactionGroupById;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Queries;

public class GetTransactionGroupByIdTests : TestBase
{
  private readonly Mock<ILogger<GetTransactionGroupByIdQueryHandler>> _loggerMock;
  private readonly GetTransactionGroupByIdQueryHandler _handler;

  public GetTransactionGroupByIdTests()
  {
    _loggerMock = CreateLoggerMock<GetTransactionGroupByIdQueryHandler>();
    _handler = new GetTransactionGroupByIdQueryHandler(
        _loggerMock.Object,
        Mapper,
        TransactionGroupRepositoryMock.Object
    );
  }

  [Fact]
  public async Task GetTransactionGroupById_ValidId_ReturnsGroup()
  {
    // arrange
    var groupId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hash", CurrencyEnum.USD);
    var group = new TransactionGroup("TestGroup", "desc", null, user);
    TransactionGroupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, true, It.IsAny<CancellationToken>())).ReturnsAsync(group);
    var query = new GetTransactionGroupByIdQuery(groupId, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    TransactionGroupRepositoryMock.Verify(x => x.GetByIdAsync(groupId, true, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task GetTransactionGroupById_GroupNotFound_ReturnsFailure()
  {
    // arrange
    var groupId = Guid.NewGuid();
    TransactionGroupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, true, It.IsAny<CancellationToken>())).ReturnsAsync((TransactionGroup)null!);
    var query = new GetTransactionGroupByIdQuery(groupId, CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    TransactionGroupRepositoryMock.Verify(x => x.GetByIdAsync(groupId, true, It.IsAny<CancellationToken>()), Times.Once);
  }
}
