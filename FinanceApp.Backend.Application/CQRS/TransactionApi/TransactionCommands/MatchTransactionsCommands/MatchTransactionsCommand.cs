using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.MatchTransactionsCommands;

public record MatchTransactionsCommand(RabbitMqPayload ResponseDto) : ICommand<Result<bool>>;
