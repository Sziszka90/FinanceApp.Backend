using FinanceApp.Backend.Domain.Common;

namespace FinanceApp.Backend.Domain.Entities;

/// <summary>Stores the shared schedule for the singleton exchange-rate refresh job.</summary>
public class ExchangeRateRefreshState : BaseEntity
{
  public static readonly Guid SingletonId = Guid.Parse("5d98edb9-7504-4d41-a2d8-e9e3be6261e3");
  public static readonly DateTimeOffset MigrationSeedTimestamp = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

  public DateTimeOffset? LastSuccessfulRefreshAt { get; set; }

  /// <summary>The next scheduled run, including any retry backoff after a failed refresh.</summary>
  public DateTimeOffset? NextAttemptAt { get; set; }

  public ExchangeRateRefreshState()
  {
    Id = SingletonId;
  }
}
