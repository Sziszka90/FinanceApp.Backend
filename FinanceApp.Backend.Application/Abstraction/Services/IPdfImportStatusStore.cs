using FinanceApp.Backend.Application.Dtos.TransactionDtos;

namespace FinanceApp.Backend.Application.Abstraction.Services;

public interface IPdfImportStatusStore
{
  Task SetAsync(string ownerEmail, Guid correlationId, string status, string? error = null,
    CancellationToken cancellationToken = default);

  Task<PdfImportStatusDto?> GetAsync(string ownerEmail, Guid correlationId,
    CancellationToken cancellationToken = default);
}
