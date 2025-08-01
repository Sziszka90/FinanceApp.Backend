using FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupQueries.GetAllTransactionGroup;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Queries;

public class GetAllTransactionGroupTests : TestBase
{
  private readonly Mock<ILogger<GetAllTransactionGroupQueryHandler>> _loggerMock;
  private readonly GetAllTransactionGroupQueryHandler _handler;

  public GetAllTransactionGroupTests()
  {
    _loggerMock = CreateLoggerMock<GetAllTransactionGroupQueryHandler>();
    _handler = new GetAllTransactionGroupQueryHandler(
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
    var query = new GetAllTransactionGroupQuery(CancellationToken.None);

    // act
    var result = await _handler.Handle(query, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    TransactionGroupRepositoryMock.Verify(x => x.GetAllAsync(true, It.IsAny<CancellationToken>()), Times.Once);
  }
}
