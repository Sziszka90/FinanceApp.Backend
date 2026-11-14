namespace FinanceApp.Backend.Application.Dtos.McpDtos;

public sealed record MonthlyTransactionContext(
  int SelectedYear,
  int SelectedMonth,
  int LookbackMonths,
  string Currency,
  DateTimeOffset RangeStart,
  DateTimeOffset RangeEndExclusive,
  IReadOnlyList<MonthlyTransactionItem> Transactions);
