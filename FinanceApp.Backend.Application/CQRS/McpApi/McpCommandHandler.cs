using FinanceApp.Backend.Application.Abstraction.Repositories;
using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.Models;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Application.McpApi.McpCommands;

public class McpCommandHandler : ICommandHandler<McpCommand, Result<McpEnvelope>>
{
  private readonly ILogger<McpCommandHandler> _logger;
  private readonly ITransactionRepository _transactionRepository;

  public McpCommandHandler(
    ILogger<McpCommandHandler> logger,
    ITransactionRepository transactionRepository)
  {
    _logger = logger;
    _transactionRepository = transactionRepository;
  }

  /// <inheritdoc />
  public async Task<Result<McpEnvelope>> Handle(McpCommand request, CancellationToken cancellationToken)
  {
    request.McpRequest.Arguments.TryGetValue("userId", out var userIdObj);
    Guid userId = ConvertUserIdToGuid(userIdObj);

    request.McpRequest.Arguments.TryGetValue("startDate", out var startDateObj);
    DateTimeOffset startDate = ConvertToDateTimeOffset(startDateObj);

    request.McpRequest.Arguments.TryGetValue("endDate", out var endDateObj);
    DateTimeOffset endDate = ConvertToDateTimeOffset(endDateObj);

    request.McpRequest.Arguments.TryGetValue("top", out var topObj);
    int top = topObj is int t ? t : 10;

    switch (request.McpRequest.Name)
    {
      case "GetTopTransactionGroups":
        {
          var transactionGroups = await _transactionRepository.GetTransactionGroupAggregatesAsync(
            userId: userId,
            startDate: startDate,
            endDate: endDate,
            topCount: top,
            cancellationToken: cancellationToken);

          _logger.LogInformation("MCP Command Handler: Successfully retrieved transaction groups for user {UserId}", userId);

          return Result.Success(new McpEnvelope
          {
            ToolName = request.McpRequest.Name,
            Payload = transactionGroups
          });
        }

      default:
        _logger.LogError("Unsupported MCP request name: {RequestName}", request.McpRequest.Name);
        return Result.Failure<McpEnvelope>(ApplicationError.DefaultError($"MCP Command Handler: Unsupported MCP request name '{request.McpRequest.Name}'"));
    }
  }

  private Guid ConvertUserIdToGuid(object? userIdObj)
  {
    if (userIdObj is Guid guid)
    {
      return guid;
    }

    if (userIdObj is string str && Guid.TryParse(str, out var parsedGuid))
    {
      return parsedGuid;
    }

    throw new ArgumentException("Value cannot be converted to Guid.", nameof(userIdObj));
  }

  private DateTimeOffset ConvertToDateTimeOffset(object? value)
  {
    if (value is DateTimeOffset dto)
    {
      return dto;
    }

    if (value is DateTime dt)
    {
      return new DateTimeOffset(dt);
    }

    if (value is string s && DateTimeOffset.TryParse(s, out var parsed))
    {
      return parsed;
    }

    throw new ArgumentException("Value cannot be converted to DateTimeOffset.", nameof(value));
  }
}
