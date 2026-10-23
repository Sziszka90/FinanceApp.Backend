using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.ImportPdf;

public record ImportPdfCommand(UploadPdfFileDto Upload) : ICommand<Result<string>>;
