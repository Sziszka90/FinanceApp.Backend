using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.McpApi.McpCommands;
using FinanceApp.Backend.Application.Models;
using FinanceApp.Backend.Application.Validators;
using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinanceApp.Backend.Testing.Unit.McpTests.Commands;

public class McpTests
{
  private readonly Mock<ILogger<McpCommandHandler>> _loggerMock = new();
  private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
  private readonly Mock<IExchangeRateRepository> _exchangeRateRepositoryMock = new();
  private readonly Mock<IUserRepository> _userRepositoryMock = new();
  private readonly Mock<IMediator> _mediatorMock = new();

  private McpCommandHandler CreateHandler() =>
    new McpCommandHandler(
      _loggerMock.Object,
      _transactionRepositoryMock.Object,
      _exchangeRateRepositoryMock.Object,
      _userRepositoryMock.Object,
      _mediatorMock.Object);

  [Fact]
  public async Task Handle_GetTopTransactionGroups_ReturnsSuccess()
  {
    // arrange
    var aggregates = new List<TopTransactionGroupDto>() {
      new TopTransactionGroupDto() {
        Name = "Group 1",
        Description = "Description",
        TransactionCount = 10,
        TotalAmount = new Money() { Amount = 200, Currency = CurrencyEnum.USD }
      }
    };

    _mediatorMock.Setup(m => m.Send(It.IsAny<IRequest<Result<List<TopTransactionGroupDto>>>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(Result.Success(aggregates));

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

    var handler = CreateHandler();
    var command = new McpCommand(mcpRequest, CancellationToken.None);

    // act
    var result = await handler.Handle(command, CancellationToken.None);

    // assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
    Assert.Equal(SupportedTools.GET_TOP_TRANSACTION_GROUPS, result.Data.ToolName);
    var payload = result.Data.Payload as List<TopTransactionGroupDto>;
    Assert.NotNull(payload);
    Assert.Equal(aggregates, payload);
  }

  [Fact]
  public async Task Handle_InvalidUserId_ThrowsArgumentException()
  {
    // arrange
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

    // act & assert
    await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command, CancellationToken.None));
  }

  [Fact]
  public void Validate_UnsupportedTool_ReturnsValidationError()
  {
    // arrange
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

    // act
    var validationResult = validator.Validate(mcpRequest);

    // assert
    Assert.False(validationResult.IsValid);
    Assert.Contains(validationResult.Errors, e => e.ErrorMessage.Contains("ToolName must be one of the supported tools."));
  }
}
