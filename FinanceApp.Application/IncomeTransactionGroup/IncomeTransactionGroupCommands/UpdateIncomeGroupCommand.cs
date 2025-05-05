using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.IncomeTransactionGroupDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransactionGroup.IncomeTransactionGroupCommands;

public record UpdateIncomeGroupCommand(UpdateIncomeTransactionGroupDto UpdateIncomeTransactionGroupDto) : ICommand<Result<GetIncomeTransactionGroupDto>>;