using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UploadCsv;

public record UploadCsvCommand(UploadCsvFileDto uploadCsvFileDto, CancellationToken CancellationToken) : ICommand<Result<List<GetTransactionDto>>>;
