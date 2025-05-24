using FinanceApp.Application.Abstractions.CQRS;
using FinanceApp.Application.Models;

namespace FinanceApp.Application.IncomeTransaction.IncomeTransactionQueries;

public record GetExchangeRateQuery : IQuery<Result<Dictionary<string, decimal>?>>;
