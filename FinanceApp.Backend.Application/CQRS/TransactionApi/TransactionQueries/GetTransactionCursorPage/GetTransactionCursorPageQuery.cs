using FinanceApp.Backend.Application.Abstractions.CQRS;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using FinanceApp.Backend.Application.Models;

namespace FinanceApp.Backend.Application.TransactionApi.TransactionQueries.GetTransactionCursorPage;

public record GetTransactionCursorPageQuery(
  int PageSize,
  string? Cursor,
  TransactionFilter? TransactionFilter,
  CancellationToken CancellationToken) : IQuery<Result<TransactionCursorPageDto>>;
