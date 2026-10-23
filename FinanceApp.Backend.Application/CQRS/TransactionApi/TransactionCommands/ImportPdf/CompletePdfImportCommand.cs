using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.RabbitMQDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.ImportPdf;

public record CompletePdfImportCommand(PdfImportResultDto Result) : ICommand<Result<bool>>;
