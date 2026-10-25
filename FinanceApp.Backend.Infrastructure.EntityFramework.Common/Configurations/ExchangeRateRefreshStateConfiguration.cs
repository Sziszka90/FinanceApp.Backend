using FinanceApp.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;

public class ExchangeRateRefreshStateConfiguration : IEntityTypeConfiguration<ExchangeRateRefreshState>
{
  public void Configure(EntityTypeBuilder<ExchangeRateRefreshState> builder)
  {
    builder.ToTable(nameof(ExchangeRateRefreshState));
    builder.HasKey(state => state.Id);
    builder.Property(state => state.LastSuccessfulRefreshAt);
    builder.Property(state => state.NextAttemptAt);
    builder.HasData(new ExchangeRateRefreshState
    {
      Id = ExchangeRateRefreshState.SingletonId,
      Created = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
      Modified = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
      LastSuccessfulRefreshAt = null,
      NextAttemptAt = null
    });
  }
}
