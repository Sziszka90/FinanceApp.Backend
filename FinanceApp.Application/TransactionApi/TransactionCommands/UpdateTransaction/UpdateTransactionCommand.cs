using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.UpdateTransaction;

public record UpdateTransactionCommand(Guid Id, UpdateTransactionDto UpdateTransactionDto, CancellationToken CancellationToken) : ICommand<Result<GetTransactionDto>>;
