using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UpdateTransaction;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class UpdateTransactionCommandHandlerTests : TestBase
{
  private readonly Mock<ILogger<UpdateTransactionCommandHandler>> _loggerMock;
  private readonly UpdateTransactionCommandHandler _handler;

  public UpdateTransactionCommandHandlerTests()
  {
    _loggerMock = CreateLoggerMock<UpdateTransactionCommandHandler>();
    _handler = new UpdateTransactionCommandHandler(
        Mapper,
        UnitOfWorkMock.Object,
        TransactionRepositoryMock.Object,
        TransactionGroupRepositoryMock.Object,
        _loggerMock.Object
    );
  }

  [Fact]
  public async Task UpdateTransaction_ValidRequest_UpdatesTransaction()
  {
    // arrange
    var user = new User(null, "testuser", "test@example.com", true, "hash", CurrencyEnum.USD);

    var transactionGroup = new TransactionGroup("Test Group", "Description", "", user);

    var transaction = new Transaction(
      "Test Transaction",
      "Description",
      TransactionTypeEnum.Income,
      new Money()
      {
        Amount = 100,
        Currency = CurrencyEnum.USD
      },
      transactionGroup,
      DateTime.UtcNow,
      user
    );

    TransactionRepositoryMock.Setup(x => x.GetByIdAsync(transaction.Id, false, It.IsAny<CancellationToken>())).ReturnsAsync(transaction);
    TransactionGroupRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(transactionGroup);
    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    var updateDto = new UpdateTransactionDto();
    var command = new UpdateTransactionCommand(transaction.Id, updateDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    TransactionRepositoryMock.Verify(x => x.GetByIdAsync(transaction.Id, false, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task UpdateTransaction_TransactionNotFound_ReturnsFailure()
  {
    // arrange
    var transactionId = Guid.NewGuid();
    TransactionRepositoryMock.Setup(x => x.GetByIdAsync(transactionId, false, It.IsAny<CancellationToken>())).ReturnsAsync((Transaction)null);
    var updateDto = new UpdateTransactionDto();
    var command = new UpdateTransactionCommand(transactionId, updateDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    TransactionRepositoryMock.Verify(x => x.GetByIdAsync(transactionId, false, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
