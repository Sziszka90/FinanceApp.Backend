using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionQueries;

public record GetIncomeByIdQuery(Guid Id) : IQuery<Result<GetIncomeTransactionDto>>;