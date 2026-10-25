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
      Created = ExchangeRateRefreshState.MigrationSeedTimestamp,
      Modified = ExchangeRateRefreshState.MigrationSeedTimestamp,
      LastSuccessfulRefreshAt = null,
      NextAttemptAt = null
    });
  }
}
