using System.Text.Json;
using FinanceApp.Backend.Application.Abstraction.Services;
using FinanceApp.Backend.Application.Dtos.TransactionDtos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FinanceApp.Backend.Infrastructure.Cache;

public class PdfImportStatusStore : IPdfImportStatusStore
{
  private static readonly TimeSpan StatusRetention = TimeSpan.FromDays(7);
  private readonly IDistributedCache _cache;
  private readonly ILogger<PdfImportStatusStore> _logger;

  public PdfImportStatusStore(IDistributedCache cache, ILogger<PdfImportStatusStore> logger)
  {
    _cache = cache;
    _logger = logger;
  }

  public async Task SetAsync(string ownerEmail, Guid correlationId, string status, string? error = null,
    CancellationToken cancellationToken = default)
  {
    var key = GetKey(ownerEmail, correlationId);
    try
    {
      var current = await GetCoreAsync(key, cancellationToken);
      if (current is not null && IsTerminal(current.Status))
      {
        return;
      }

      var value = new PdfImportStatusDto(correlationId.ToString(), status, error, DateTimeOffset.UtcNow);
      await _cache.SetStringAsync(key, JsonSerializer.Serialize(value), new DistributedCacheEntryOptions
      {
        AbsoluteExpirationRelativeToNow = StatusRetention
      }, cancellationToken);
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
      _logger.LogError(exception, "Could not store PDF import status for correlation {CorrelationId}", correlationId);
    }
  }

  public async Task<PdfImportStatusDto?> GetAsync(string ownerEmail, Guid correlationId,
    CancellationToken cancellationToken = default)
  {
    try
    {
      return await GetCoreAsync(GetKey(ownerEmail, correlationId), cancellationToken);
    }
    catch (Exception exception) when (exception is not OperationCanceledException)
    {
      _logger.LogError(exception, "Could not read PDF import status for correlation {CorrelationId}", correlationId);
      return null;
    }
  }

  private async Task<PdfImportStatusDto?> GetCoreAsync(string key, CancellationToken cancellationToken)
  {
    var value = await _cache.GetStringAsync(key, cancellationToken);
    return value is null ? null : JsonSerializer.Deserialize<PdfImportStatusDto>(value);
  }

  private static string GetKey(string ownerEmail, Guid correlationId) =>
    $"pdf-import-status:{ownerEmail.Trim().ToLowerInvariant()}:{correlationId:N}";

  private static bool IsTerminal(string status) => status is "completed" or "failed";
}
