using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UploadCsv;

public record LLMProcessorCommand(RabbitMqPayload<MatchTransactionResponseDto> ResponseDto) : ICommand<Result<bool>>;
