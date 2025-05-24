using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionCommands;

public record UpdateIncomeCommand(UpdateIncomeTransactionDto UpdateIncomeTransactionDto) : ICommand<Result<GetIncomeTransactionDto>>;
