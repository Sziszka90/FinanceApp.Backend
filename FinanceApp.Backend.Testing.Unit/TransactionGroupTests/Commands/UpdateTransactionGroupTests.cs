using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.UpdateTransactionGroup;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class UpdateTransactionGroupTests : TestBase
{
  private readonly Mock<ILogger<UpdateTransactionGroupCommandHandler>> _loggerMock;
  private readonly UpdateTransactionGroupCommandHandler _handler;

  public UpdateTransactionGroupTests()
  {
    _loggerMock = CreateLoggerMock<UpdateTransactionGroupCommandHandler>();
    _handler = new UpdateTransactionGroupCommandHandler(
        _loggerMock.Object,
        Mapper,
        UnitOfWorkMock.Object,
        TransactionGroupRepositoryMock.Object
    );
  }

  [Fact]
  public async Task UpdateTransactionGroup_GroupNotFound_ReturnsFailure()
  {
    // arrange
    var groupId = Guid.NewGuid();
    TransactionGroupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, false, It.IsAny<CancellationToken>())).ReturnsAsync((TransactionGroup)null!);
    var updateDto = new UpdateTransactionGroupDto();
    var command = new UpdateTransactionGroupCommand(groupId, updateDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    TransactionGroupRepositoryMock.Verify(x => x.GetByIdAsync(groupId, false, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task UpdateTransactionGroup_DuplicateName_ReturnsFailure()
  {
    // arrange
    var groupId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hash", CurrencyEnum.USD);
    var group = new TransactionGroup("TestGroup", "desc", null, user);
    TransactionGroupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, false, It.IsAny<CancellationToken>())).ReturnsAsync(group);
    TransactionGroupRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<TransactionGroup>>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(new List<TransactionGroup> { new TransactionGroup("TestGroup", "desc", null, user) });
    var updateDto = new UpdateTransactionGroupDto();
    var command = new UpdateTransactionGroupCommand(groupId, updateDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    TransactionGroupRepositoryMock.Verify(x => x.GetQueryAsync(It.IsAny<QueryCriteria<TransactionGroup>>(), true, It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task UpdateTransactionGroup_ValidUpdate_UpdatesGroup()
  {
    var groupId = Guid.NewGuid();
    var user = new User("TestUser", "test@example.com", "hash", CurrencyEnum.USD);
    var group = new TransactionGroup("TestGroup", "desc", null, user);
    TransactionGroupRepositoryMock.Setup(x => x.GetByIdAsync(groupId, false, It.IsAny<CancellationToken>())).ReturnsAsync(group);
    TransactionGroupRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<TransactionGroup>>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(new List<TransactionGroup>());
    UnitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    var updateDto = new UpdateTransactionGroupDto();
    var command = new UpdateTransactionGroupCommand(groupId, updateDto, CancellationToken.None);
    var result = await _handler.Handle(command, CancellationToken.None);
    Assert.True(result.IsSuccess);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }
}
