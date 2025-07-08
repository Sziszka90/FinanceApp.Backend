using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.Transaction.TransactionCommands;

public record UpdateTransactionCommand(Guid Id, UpdateTransactionDto UpdateTransactionDto) : ICommand<Result<GetTransactionDto>>;
