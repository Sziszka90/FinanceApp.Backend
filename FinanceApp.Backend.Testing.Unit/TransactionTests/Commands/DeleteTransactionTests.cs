using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.DeleteTransaction;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class DeleteTransactionTests : TestBase
{
  private readonly Mock<ILogger<DeleteTransactionCommandHandler>> _loggerMock;
  private readonly DeleteTransactionCommandHandler _handler;

  public DeleteTransactionTests()
  {
    _loggerMock = CreateLoggerMock<DeleteTransactionCommandHandler>();
    _handler = new DeleteTransactionCommandHandler(
        _loggerMock.Object,
        TransactionRepositoryMock.Object,
        UnitOfWorkMock.Object
    );
  }

  [Fact]
  public async Task DeleteTransaction_ValidRequest_DeletesTransaction()
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
      100m,
      new TransactionGroup("Test Group", "Description", "", user),
      DateTimeOffset.UtcNow,
      user
    );

    TransactionRepositoryMock.Setup(x => x.GetByIdAsync(transaction.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
    TransactionRepositoryMock.Setup(x => x.Delete(transaction));
    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    var command = new DeleteTransactionCommand(transaction.Id, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    TransactionRepositoryMock.Verify(x => x.GetByIdAsync(transaction.Id, true, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.Delete(transaction), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task DeleteTransaction_TransactionNotFound_ReturnsFailure()
  {
    // arrange
    var transactionId = Guid.NewGuid();
    TransactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>())).ReturnsAsync((Transaction)null!);
    var command = new DeleteTransactionCommand(transactionId, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    TransactionRepositoryMock.Verify(x => x.GetByIdAsync(transactionId, true, It.IsAny<CancellationToken>()), Times.Once);
    TransactionRepositoryMock.Verify(x => x.Delete(It.IsAny<Transaction>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
