using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.RabbitMQDtos;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.UploadCsv;

public record LLMProcessorCommand(RabbitMQResponseDto ResponseDto ) : ICommand<Result<List<GetTransactionDto>>>;
