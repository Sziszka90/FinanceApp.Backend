using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;
using Microsoft.AspNetCore.Http;

namespace FinanceApp.Application.TransactionGroupApi.TransactionGroupCommands.CreateTransactionGroup;

public record CreateTransactionGroupCommand(CreateTransactionGroupDto CreateTransactionGroupDto) : ICommand<Result<GetTransactionGroupDto>>;
