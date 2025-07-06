using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.SaltEdgeDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.SaltEdge.SaltEdgeCommands;

public record CreateSaltEdgeNotifyCommand(CreateSaltEdgeNotifyRequestDto CreateSaltEdgeNotifyDto) : ICommand<Result>;
