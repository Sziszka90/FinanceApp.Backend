using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.McpApi.McpCommands;

public record McpCommand(McpRequest McpRequest, CancellationToken CancellationToken) : ICommand<Result<McpEnvelope>>;
