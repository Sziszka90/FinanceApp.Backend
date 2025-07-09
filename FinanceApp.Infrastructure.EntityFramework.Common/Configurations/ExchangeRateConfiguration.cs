using FinanceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Infrastructure.EntityFramework.Common.Configurations;

public class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
  /// <inheritdoc />
  public void Configure(EntityTypeBuilder<ExchangeRate> builder)
  {
    builder.HasKey(x => x.Id);
    builder.Property(x => x.BaseCurrency).IsRequired().HasMaxLength(3);
    builder.Property(x => x.TargetCurrency).IsRequired().HasMaxLength(3);
    builder.Property(x => x.Rate).IsRequired().HasColumnType("decimal(18,6)");
  }
}
