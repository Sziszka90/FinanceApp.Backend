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
    request.McpRequest.Parameters.TryGetValue("UserId", out var userIdObj);
    Guid userId = ConvertUserIdToGuid(userIdObj);

    request.McpRequest.Parameters.TryGetValue("StartDate", out var startDateObj);
    DateTimeOffset startDate = ConvertToDateTimeOffset(startDateObj);

    request.McpRequest.Parameters.TryGetValue("EndDate", out var endDateObj);
    DateTimeOffset endDate = ConvertToDateTimeOffset(endDateObj);

    request.McpRequest.Parameters.TryGetValue("Top", out var topObj);
    int top = ConvertToInt(topObj);

    switch (request.McpRequest.ToolName)
    {
      case SupportedTools.GET_TOP_TRANSACTION_GROUPS:
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
            ToolName = request.McpRequest.ToolName,
            Payload = transactionGroups
          });
        }

      default:
        _logger.LogError("Unsupported MCP request tool: {RequestTool}", request.McpRequest.ToolName);
        return Result.Failure<McpEnvelope>(ApplicationError.DefaultError($"MCP Command Handler: Unsupported MCP request tool '{request.McpRequest.ToolName}'"));
    }
  }

  private Guid ConvertUserIdToGuid(object? value)
  {
    if (value is Guid guid)
    {
      return guid;
    }

    if (value is string str && Guid.TryParse(str, out var parsedGuid))
    {
      return parsedGuid;
    }

    if (value is System.Text.Json.JsonElement json)
    {
      if (json.ValueKind == System.Text.Json.JsonValueKind.String && Guid.TryParse(json.GetString(), out var jsonGuid))
      {
        return jsonGuid;
      }
    }

    throw new ArgumentException("Value cannot be converted to Guid.", nameof(value));
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

    if (value is System.Text.Json.JsonElement json)
    {
      if (json.ValueKind == System.Text.Json.JsonValueKind.String && DateTimeOffset.TryParse(json.GetString(), out var jsonDateTime))
      {
        return jsonDateTime;
      }
    }

    throw new ArgumentException("Value cannot be converted to DateTimeOffset.", nameof(value));
  }

  private int ConvertToInt(object? value)
  {
    if (value is int i)
    {
      return i;
    }

    if (value is string s && int.TryParse(s, out var parsedInt))
    {
      return parsedInt;
    }

    if (value is System.Text.Json.JsonElement json)
    {
      if (json.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(json.GetString(), out var jsonIntStr))
      {
        return jsonIntStr;
      }

      if (json.ValueKind == System.Text.Json.JsonValueKind.Number && json.TryGetInt32(out var jsonInt))
      {
        return jsonInt;
      }
    }

    throw new ArgumentException("Value cannot be converted to int.", nameof(value));
  }
}
