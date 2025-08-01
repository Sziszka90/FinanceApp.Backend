using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.TransactionTests.Commands;

public class CreateTransactionGroupTests : TestBase
{
  private readonly Mock<ILogger<CreateTransactionGroupCommandHandler>> _loggerMock;
  private readonly CreateTransactionGroupCommandHandler _handler;

  public CreateTransactionGroupTests()
  {
    _loggerMock = CreateLoggerMock<CreateTransactionGroupCommandHandler>();
    _handler = new CreateTransactionGroupCommandHandler(
      _loggerMock.Object,
      Mapper,
      TransactionGroupRepositoryMock.Object,
      UserRepositoryMock.Object,
      UnitOfWorkMock.Object,
      HttpContextAccessorMock.Object
    );
  }

  [Fact]
  public async Task CreateTransactionGroup_ValidRequest_CreatesGroup()
  {
    // arrange
    var user = new User("TestUser", "test@example.com", "hash", CurrencyEnum.USD);

    var dto = new CreateTransactionGroupDto
    {
      Name = "TestGroup",
      Description = "desc",
      GroupIcon = null,
    };

    var group = new TransactionGroup(dto.Name, dto.Description, dto.GroupIcon, user);

    TransactionGroupRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<TransactionGroup>(), It.IsAny<CancellationToken>())).ReturnsAsync(group);
    TransactionGroupRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<TransactionGroup>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    UserRepositoryMock.Setup(x => x.GetUserByEmailAsync(user.Email, false, It.IsAny<CancellationToken>())).ReturnsAsync(user);

    var command = new CreateTransactionGroupCommand(dto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    TransactionGroupRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TransactionGroup>(), It.IsAny<CancellationToken>()), Times.Once);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  [Fact]
  public async Task CreateTransactionGroup_DuplicateName_ReturnsFailure()
  {
    // arrange
    var user = new User("TestUser", "test@example.com", "hash", CurrencyEnum.USD);

    var createTransactionGroupDto = new CreateTransactionGroupDto
    {
      Name = "TestGroup",
      Description = "desc",
      GroupIcon = null,
    };

    var existingGroup = new TransactionGroup(createTransactionGroupDto.Name, createTransactionGroupDto.Description, createTransactionGroupDto.GroupIcon, user);
    TransactionGroupRepositoryMock.Setup(x => x.GetQueryAsync(It.IsAny<QueryCriteria<TransactionGroup>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync(new List<TransactionGroup> { existingGroup });
    var command = new CreateTransactionGroupCommand(createTransactionGroupDto, CancellationToken.None);

    // act
    var result = await _handler.Handle(command, CancellationToken.None);

    // assert
    Assert.False(result.IsSuccess);
    TransactionGroupRepositoryMock.Verify(x => x.GetQueryAsync(It.IsAny<QueryCriteria<TransactionGroup>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    TransactionGroupRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<TransactionGroup>(), It.IsAny<CancellationToken>()), Times.Never);
    UnitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
  }
}
