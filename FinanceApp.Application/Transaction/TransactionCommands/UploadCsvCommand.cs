using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public record UploadCsvCommand(UploadCsvFileDto uploadCsvFileDto) : ICommand<Result<List<GetTransactionDto>>>;
