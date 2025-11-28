using FinanceApp.Backend.Domain.Common;

namespace FinanceApp.Backend.Domain.Entities;

public class MatchTransaction : BaseEntity
{
  /// <summary>
  /// Transaction name
  /// </summary>
  public required string Transaction { get; set; }

  /// <summary>
  /// Matched transaction group
  /// </summary>
  public required string TransactionGroup { get; set; }

  /// <summary>
  /// Correlation ID
  /// </summary>
  public required string CorrelationId { get; set; }
}
