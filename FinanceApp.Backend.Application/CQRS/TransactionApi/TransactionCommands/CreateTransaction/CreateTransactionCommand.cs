using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionCommands.CreateTransaction;

public record CreateTransactionCommand(CreateTransactionDto CreateTransactionDto, CancellationToken CancellationToken) : ICommand<Result<GetTransactionDto>>;
