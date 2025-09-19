using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.McpApi.McpCommands;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Validators;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.McpTests.Commands;

public class McpTests
{
  private readonly Mock<ILogger<McpCommandHandler>> _loggerMock = new();
  private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
  private readonly Mock<IExchangeRateRepository> _exchangeRateRepositoryMock = new();
  private readonly Mock<IUserRepository> _userRepositoryMock = new();

  private McpCommandHandler CreateHandler() =>
      new McpCommandHandler(_loggerMock.Object, _transactionRepositoryMock.Object, _exchangeRateRepositoryMock.Object, _userRepositoryMock.Object);

  [Fact]
  public async Task Handle_GetTopTransactionGroups_ReturnsSuccess()
  {
    var userId = Guid.NewGuid();
    var mcpRequest = new McpRequest
    {
      ToolName = SupportedTools.GET_TOP_TRANSACTION_GROUPS,
      Parameters = new Dictionary<string, object>
      {
        { "UserId", userId },
        { "StartDate", DateTimeOffset.Now.AddDays(-30) },
        { "EndDate", DateTimeOffset.Now },
        { "Top", 5 }
      }
    };

    var aggregates = new List<TransactionGroupAggregate> { };

    _transactionRepositoryMock
      .Setup(x => x.GetTransactionGroupAggregatesAsync(
          userId,
          It.IsAny<DateTimeOffset>(),
          It.IsAny<DateTimeOffset>(),
          5,
          It.IsAny<bool>(),
          It.IsAny<CancellationToken>()
      ))
      .Returns(Task.FromResult(aggregates));

    _userRepositoryMock
      .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
      .Returns(Task.FromResult<User?>(
        new User(
          userId,
          "testuser",
          "testuser@example.com",
          true,
          "hash",
          CurrencyEnum.USD)));

    var handler = CreateHandler();
    var command = new McpCommand(mcpRequest, CancellationToken.None);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(SupportedTools.GET_TOP_TRANSACTION_GROUPS, result.Data.ToolName);
    var payload = result.Data.Payload as List<TransactionGroupAggregate>;
    Assert.NotNull(payload);
    Assert.Equal(aggregates, payload);
  }

  [Fact]
  public async Task Handle_InvalidUserId_ThrowsArgumentException()
  {
    var mcpRequest = new McpRequest
    {
      ToolName = "GetTopTransactionGroups",
      Parameters = new Dictionary<string, object>
            {
                { "UserId", "not-a-guid" },
                { "StartDate", DateTimeOffset.Now.AddDays(-30) },
                { "EndDate", DateTimeOffset.Now },
                { "Top", 5 }
            }
    };

    var handler = CreateHandler();
    var command = new McpCommand(mcpRequest, CancellationToken.None);

    await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
  }

  [Fact]
  public void Validate_UnsupportedTool_ReturnsValidationError()
  {
    var mcpRequest = new McpRequest
    {
      ToolName = "unsupported_tool",
      Parameters = new Dictionary<string, object>
      {
        { "UserId", Guid.NewGuid() },
        { "CorrelationId", Guid.NewGuid() }
      }
    };

    var validator = new McpRequestValidator();
    var validationResult = validator.Validate(mcpRequest);

    Assert.False(validationResult.IsValid);
    Assert.Contains(validationResult.Errors, e => e.ErrorMessage.Contains("ToolName must be one of the supported tools."));
  }
}
