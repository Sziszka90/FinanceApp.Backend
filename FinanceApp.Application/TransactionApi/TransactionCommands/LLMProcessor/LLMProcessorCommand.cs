using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.RabbitMQDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.UploadCsv;

public record LLMProcessorCommand(RabbitMqPayload ResponseDto ) : ICommand<Result<bool>>;
