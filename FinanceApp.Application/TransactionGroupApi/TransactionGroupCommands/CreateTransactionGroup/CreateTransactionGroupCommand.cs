using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;

public record CreateTransactionGroupCommand(CreateTransactionGroupDto CreateTransactionGroupDto, CancellationToken CancellationToken) : ICommand<Result<GetTransactionGroupDto>>;
