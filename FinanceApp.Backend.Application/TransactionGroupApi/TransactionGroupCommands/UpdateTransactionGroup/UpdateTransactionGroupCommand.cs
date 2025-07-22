using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.UpdateTransactionGroup;

public record UpdateTransactionGroupCommand(Guid Id, UpdateTransactionGroupDto UpdateTransactionGroupDto, CancellationToken CancellationToken) : ICommand<Result<GetTransactionGroupDto>>;
