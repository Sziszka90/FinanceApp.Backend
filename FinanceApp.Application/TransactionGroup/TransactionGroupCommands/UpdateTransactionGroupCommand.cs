using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupCommands;

public record UpdateTransactionGroupCommand(UpdateTransactionGroupDto UpdateTransactionGroupDto) : ICommand<Result<GetTransactionGroupDto>>;
