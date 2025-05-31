using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.TransactionGroupDtos;
using FinanceApp.Application.Models;
using Microsoft.AspNetCore.Http;

namespace FinanceApp.Application.TransactionGroup.TransactionGroupCommands;

public record CreateTransactionGroupCommand(CreateTransactionGroupDto CreateTransactionGroupDto, IFormFile? Image) : ICommand<Result<GetTransactionGroupDto>>;
