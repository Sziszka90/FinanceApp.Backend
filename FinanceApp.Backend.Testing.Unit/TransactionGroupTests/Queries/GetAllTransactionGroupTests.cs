using FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetAllTransactionGroups;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Queries;

public class GetAllTransactionGroupTests : TestBase
{
  private readonly Mock<ILogger<GetAllTransactionGroupsQueryHandler>> _loggerMock;
  private readonly GetAllTransactionGroupsQueryHandler _handler;

  public GetAllTransactionGroupTests()
  {
    _loggerMock = CreateLoggerMock<GetAllTransactionGroupsQueryHandler>();
    _handler = new GetAllTransactionGroupsQueryHandler(
        _loggerMock.Object,
        Mapper,
        TransactionGroupRepositoryMock.Object
    );
  }

  [Fact]
  public async Task GetAllTransactionGroups_ReturnsGroups()
  {
    // arrange
    var user = new User("TestUser", "test@example.com", "hash", CurrencyEnum.USD);
    TransactionGroupRepositoryMock.Setup(x => x.GetAllAsync(true, It.IsAny<CancellationToken>())).ReturnsAsync(new List<TransactionGroup> { new TransactionGroup("TestGroup", "desc", null, user) });
    var query = new GetAllTransactionGroupsQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    TransactionGroupRepositoryMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
  }
}
