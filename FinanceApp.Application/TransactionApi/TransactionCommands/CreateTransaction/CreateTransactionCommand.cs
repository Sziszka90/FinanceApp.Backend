using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionApi.TransactionCommands.CreateTransaction;

public record CreateTransactionCommand(CreateTransactionDto CreateTransactionDto) : ICommand<Result<GetTransactionDto>>;
