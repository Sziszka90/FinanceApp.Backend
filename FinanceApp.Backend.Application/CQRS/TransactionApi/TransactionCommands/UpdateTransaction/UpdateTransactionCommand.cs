using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.UpdateTransaction;

public record UpdateTransactionCommand(Guid Id, UpdateTransactionDto UpdateTransactionDto, CancellationToken CancellationToken) : ICommand<Result<GetTransactionDto>>;
