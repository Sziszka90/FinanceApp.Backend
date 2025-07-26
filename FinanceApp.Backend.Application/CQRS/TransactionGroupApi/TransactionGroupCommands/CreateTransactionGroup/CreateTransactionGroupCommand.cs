using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;

public record CreateTransactionGroupCommand(CreateTransactionGroupDto CreateTransactionGroupDto, CancellationToken CancellationToken) : ICommand<Result<GetTransactionGroupDto>>;
