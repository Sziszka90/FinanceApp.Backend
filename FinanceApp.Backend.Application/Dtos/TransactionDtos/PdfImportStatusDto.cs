namespace FinanceApp.Backend.Application.Dtos.TransactionDtos;

public record PdfImportStatusDto(string CorrelationId, string Status, string? Error, DateTimeOffset UpdatedAt);
