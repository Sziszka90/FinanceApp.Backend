using FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.DeleteTransactionGroup;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class DeleteTransactionGroupTests : TestBase
{
  private readonly Mock<ILogger<DeleteTransactionGroupCommandHandler>> _loggerMock;
  private readonly DeleteTransactionGroupCommandHandler _handler;

  public DeleteTransactionGroupTests()
  {
    _loggerMock = CreateLoggerMock<DeleteTransactionGroupCommandHandler>();
    _handler = new DeleteTransactionGroupCommandHandler(
        _loggerMock.Object,
        TransactionGroupRepositoryMock.Object,
        TransactionRepositoryMock.Object,
        UnitOfWorkMock.Object
    );
  }

  [Fact]
  public async Task DeleteTransactionGroup_GroupUsed_ReturnsFailure()
  {
    // arrange
    var groupId = Guid.NewGuid();
    TransactionRepositoryMock.Setup(x => x.TransactionGroupUsedAsync(groupId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    var command = new DeleteTransactionGroupCommand(groupId, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    TransactionRepositoryMock.Verify(x => x.TransactionGroupUsedAsync(groupId, It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<TransactionGroup>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task DeleteTransactionGroup_GroupNotFound_ReturnsFailure()
  {
    // arrange
    var groupId = Guid.NewGuid();
    TransactionRepositoryMock.Setup(x => x.TransactionGroupUsedAsync(groupId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    TransactionGroupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, true, It.IsAny<CancellationToken>())).ReturnsAsync((TransactionGroup)null!);
    var command = new DeleteTransactionGroupCommand(groupId, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    TransactionGroupRepositoryMock.Verify(x => x.GetByIdAsync(groupId, true, It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<TransactionGroup>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  public async Task Handle_ValidGroup_DeletesGroup()
  {
    // arrange
    var groupId = Guid.NewGuid();
    TransactionRepositoryMock.Setup(x => x.TransactionGroupUsedAsync(groupId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    var user = new User("TestUser", "test@example.com", "hash", CurrencyEnum.USD);
    var group = new TransactionGroup("TestGroup", "desc", null, user);
    TransactionGroupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, true, It.IsAny<CancellationToken>())).ReturnsAsync(group);
    TransactionGroupRepositoryMock.Setup(x => x.DeleteAsync(group, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    var command = new DeleteTransactionGroupCommand(groupId, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    TransactionGroupRepositoryMock.Verify(x => x.DeleteAsync(group, It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
