using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;
using FinanceApp.Domain.Entities;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionQueries;

public record GetIncomeSumQuery : IQuery<Result<Money>>;
