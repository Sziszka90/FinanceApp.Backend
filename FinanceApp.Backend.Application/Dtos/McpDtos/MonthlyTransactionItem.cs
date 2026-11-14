namespace FinanceApp.Backend.Application.Dtos.McpDtos;

public sealed record MonthlyTransactionItem(
  Guid Id,
  string Name,
  string? Description,
  DateTimeOffset TransactionDate,
  string? TransactionType,
  decimal Amount,
  string Currency,
  Guid? TransactionGroupId,
  string? TransactionGroupName,
  bool IsInSelectedMonth);
