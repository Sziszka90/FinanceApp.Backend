using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.McpDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetMonthlyTransactionContext;

public sealed record GetMonthlyTransactionContextQuery(
  int Year,
  int Month,
  int LookbackMonths = 6)
  : IQuery<Result<MonthlyTransactionContext>>;
