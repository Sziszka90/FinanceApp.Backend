using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.UpdateTransactionGroup;

public record UpdateTransactionGroupCommand(Guid Id, UpdateTransactionGroupDto UpdateTransactionGroupDto) : ICommand<Result<GetTransactionGroupDto>>;
