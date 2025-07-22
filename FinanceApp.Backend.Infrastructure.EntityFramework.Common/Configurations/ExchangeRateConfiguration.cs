using FinanceApp.Backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceApp.Backend.Infrastructure.EntityFramework.Common.Configurations;

public class ExchangeRateConfiguration : BaseEntityTypeConfiguration<ExchangeRate>
{
  /// <inheritdoc />
  protected override void ConfigureSpecificProperties(EntityTypeBuilder<ExchangeRate> builder)
  {
    builder.ToTable(nameof(ExchangeRate));
    builder.HasKey(x => x.Id);
    builder.Property(x => x.BaseCurrency).IsRequired().HasMaxLength(3);
    builder.Property(x => x.TargetCurrency).IsRequired().HasMaxLength(3);
    builder.Property(x => x.Rate).IsRequired().HasColumnType("decimal(18,6)");
  }
}
