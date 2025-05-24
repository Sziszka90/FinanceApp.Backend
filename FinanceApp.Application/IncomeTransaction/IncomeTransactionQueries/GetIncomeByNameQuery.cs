using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Dtos.IncomeTransactionDtos;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionQueries;

public record GetIncomeByNameQuery(Guid Id) : IQuery<Result<GetIncomeTransactionDto>>;
